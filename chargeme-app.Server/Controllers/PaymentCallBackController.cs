using chargeme_app.Server.DataContext;
using chargeme_app.Server.Models;
using chargeme_app.Server.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using System.Net;

namespace chargeme_app.Server.Controllers
{
    [ApiController]
    [Route("")]
    public class PaymentCallBackController : ControllerBase
    {
        private readonly ILogger<PaymentCallBackController> _logger;
        private readonly NpgsqlDbContext _context;
        private readonly string _merchantId;
        private readonly MemoryCacheService _cachService;
        private readonly OCPPService _ocppService;
        private readonly IConfiguration _config;
        public PaymentCallBackController(IConfiguration configuration, NpgsqlDbContext context, ILogger<PaymentCallBackController> logger, MemoryCacheService cachService, OCPPService ocppService)
        {
            _context = context;
            _cachService = cachService;
            _config = configuration;
            _logger = logger;
            _ocppService = ocppService;
            var api = _context.TblPaymentApis.FirstOrDefault();
            _merchantId = api.FMerchantId;
        }
        // กำหนด Route โดยใช้ค่า TransactionPath จากการตั้งค่า
        [HttpPost("{_merchantId}")]
        //[Route("/{_merchantId}")]
        public async Task<IActionResult> PostTransaction([FromForm] PaymentGwRequest request)
        {
            // ประมวลผลข้อมูลที่ได้รับ
            var payment = await _context.TblPayments.FirstOrDefaultAsync(x => x.FPaymentCode == request.RefNo);
            if (payment == null)
            {
                return NotFound(new { message = "Payment not found." });
            }

            // อัปเดตสถานะการชำระเงิน
            payment.FPaymentStatus = "Paid";
            payment.FUpdated = DateTime.UtcNow;
            _context.TblPayments.Update(payment);
            await _context.SaveChangesAsync(); // ✅ รอให้การบันทึกเสร็จ

            // ดึงข้อมูล transaction ที่เกี่ยวข้อง
            var trans = await _context.TblTransactions.FirstOrDefaultAsync(x => x.FId == payment.FTransactionId);
            if (trans == null)
            {
                return NotFound(new { message = "Transaction not found." });
            }

            // อัปเดต connector
            var connector = await _context.TblConnectorStatuses.FirstOrDefaultAsync(x => x.FId == trans.FConnectorId);
            if (connector != null)
            {
                connector.FTransactionId = trans.FId;
                _context.TblConnectorStatuses.Update(connector);
                await _context.SaveChangesAsync(); // ✅ รอให้การบันทึกเสร็จ
            }

            // อัปเดตหมายเลขธุรกรรม
            trans.FTransactionNo = long.Parse(payment.FOrderNo);
            _context.TblTransactions.Update(trans);
            await _context.SaveChangesAsync(); // ✅ รอให้การบันทึกเสร็จ

            // อัปเดตแคช
            await _cachService.RefreshCachePaymentIfDatabaseUpdated(payment.FId);

            try
            {
                var charger = await _context.TblChargers.FirstOrDefaultAsync(x => x.FId == trans.FChargerId);
                var station = await _context.TblStations.FirstOrDefaultAsync(x => x.FId == trans.FStationId);
                if (charger != null && station != null)
                {
                    await _ocppService.StartChargingSession(trans.FChargerId, charger.FCode, (int)connector.FConnectorId, station.FRfid);
                }

                // ส่งสถานะตอบกลับไปยัง client
                return Ok(new { message = "Transaction received successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception happened: " + ex.ToString());
                return StatusCode(500, new { message = "Internal Server Error" });
            }
            
            //_ = RemoteStartTransaction(trans.FChargerId, trans.FConnectorId);
        }

        private async Task<IActionResult> RemoteStartTransaction(Guid FChargerId, string FCode, int FConnectorId, string FRfid)
        {
            try
            {
                var message = await _ocppService.StartTransaction(FChargerId, FCode, (int)FConnectorId, FRfid);
                return Ok(new { message = "Transaction start successfully. " + message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.ToString() });
            }
        }

        [HttpPost("{_merchantId}/start-transaction")]
        public async Task<IActionResult> StartTransaction(Guid chargerId, Guid? connectorId)
        {
            int httpStatuscode = (int)HttpStatusCode.OK;
            string resultContent = "";
            var charger = await _context.TblChargers.FirstOrDefaultAsync(x => x.FId == chargerId);
            var connector = await _context.TblConnectorStatuses.FirstOrDefaultAsync(x => x.FId == connectorId && x.FChargerId == chargerId);
            var station = await _context.TblStations.FirstOrDefaultAsync(x => x.FId == connector.FStationId);
            if (connector != null)
            {
                string serverApiUrl = _config.GetValue<string>("ServerApiUrl");
                string apiKeyConfig = _config.GetValue<string>("ApiKey");
                if (!string.IsNullOrEmpty(serverApiUrl))
                {
                    try
                    {
                        using (var httpClient = new HttpClient())
                        {
                            if (!serverApiUrl.EndsWith('/'))
                            {
                                serverApiUrl += "/";
                            }
                            Uri uri = new Uri(serverApiUrl);
                            uri = new Uri(uri, $"RemoteStartTransaction/{Uri.EscapeUriString(connector.FCode)}/{Uri.EscapeUriString(connector.FCode)}/{Uri.EscapeUriString(station.FRfid)}");
                            httpClient.Timeout = new TimeSpan(0, 0, 4); // use short timeout

                            // API-Key authentication?
                            if (!string.IsNullOrWhiteSpace(apiKeyConfig))
                            {
                                httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKeyConfig);
                            }
                            else
                            {
                                _logger.LogWarning("RemoteStartTransaction: No API-Key configured!");
                            }

                            HttpResponseMessage response = await httpClient.GetAsync(uri);
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                string jsonResult = await response.Content.ReadAsStringAsync();
                                if (!string.IsNullOrEmpty(jsonResult))
                                {
                                    try
                                    {
                                        dynamic jsonObject = JsonConvert.DeserializeObject(jsonResult);
                                        _logger.LogInformation("RemoteStartTransaction: Result of API request is '{0}'", jsonResult);
                                        string status = jsonObject.status;
                                        switch (status)
                                        {
                                            case "Accepted":
                                                resultContent = "The charging station has been start.";
                                                break;
                                            case "Rejected":
                                                resultContent = "The charging station could NOT be start.";
                                                break;
                                            default:
                                                resultContent = string.Format("The charging station returned an unexpected result: '{0}'", status);
                                                break;
                                        }
                                    }
                                    catch (Exception exp)
                                    {
                                        _logger.LogError(exp, "RemoteStartTransaction: Error in JSON result => {0}", exp.Message);
                                        httpStatuscode = (int)HttpStatusCode.OK;
                                        resultContent = "An error has occurred.";
                                    }
                                }
                                else
                                {
                                    _logger.LogError("RemoteStartTransaction: Result of API request is empty");
                                    httpStatuscode = (int)HttpStatusCode.OK;
                                    resultContent = "An error has occurred.";
                                }
                            }
                            else if (response.StatusCode == HttpStatusCode.NotFound)
                            {
                                // Chargepoint offline
                                httpStatuscode = (int)HttpStatusCode.OK;
                                resultContent = "The charging station is offline and cannot be start.";
                            }
                            else
                            {
                                _logger.LogError("RemoteStartTransaction: Result of API  request => httpStatus={0}", response.StatusCode);
                                httpStatuscode = (int)HttpStatusCode.OK;
                                resultContent = "An error has occurred.";
                            }
                        }
                    }
                    catch (Exception exp)
                    {
                        _logger.LogError(exp, "RemoteStartTransaction: Error in API request => {0}", exp.Message);
                        httpStatuscode = (int)HttpStatusCode.OK;
                        resultContent = "An error has occurred.";
                    }
                }
            }
            else
            {
                _logger.LogWarning("RemoteStartTransaction: Error loading charge point '{0}' from database", charger.FCode);
                httpStatuscode = (int)HttpStatusCode.OK;
                resultContent = "The charging station returned an unexpected result: '{0}'";
            }

            return StatusCode(httpStatuscode, resultContent);
            }
    }
}

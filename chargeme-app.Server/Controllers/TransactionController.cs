using chargeme_app.Server.DataContext;
using chargeme_app.Server.Helper;
using chargeme_app.Server.Models;
using chargeme_app.Server.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using static System.Collections.Specialized.BitVector32;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace chargeme_app.Server.Controllers
{
    /// <summary>
    /// Controller responsible for handling electric vehicle charging transactions and payments.
    /// Provides endpoints for transaction history, payment processing, and status checking.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly NpgsqlDbContext _context;
        private readonly MemoryCacheService _cachService;
        private readonly OCPPService _ocppService;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the TransactionController.
        /// </summary>
        /// <param name="context">The database context for data access</param>
        /// <param name="cachService">The memory cache service for temporary data storage</param>
        public TransactionController(NpgsqlDbContext context, MemoryCacheService cachService, OCPPService ocppService, IConfiguration configuration)
        {
            _context = context;
            _cachService = cachService;
            _ocppService = ocppService;
            _configuration = configuration;
        }

        /// <summary>
        /// Retrieves the transaction history for the authenticated user.
        /// Returns a list of all completed charging sessions with station and charger details.
        /// </summary>
        /// <returns>JSON response containing transaction list with formatted data</returns>
        [HttpPost("trans-list")]
        public IActionResult GetTranslationsList()
        {
            var userID = User?.FindFirstValue(JwtRegisteredClaimNames.Sid);
            var trans = _context.TblTransactions.Where(x => x.FUserId == Guid.Parse(userID) && x.FTransactionStatus.ToLower() != "cancel").ToList();
            var translist = new List<object>();
            foreach (var item in trans.OrderByDescending(x => x.FStartTime).ToList())
            {
                var chargers = from t in _context.TblChargers
                               join l in _context.TblStations on t.FStationId equals l.FId
                               join h in _context.TblConnectorStatuses on t.FId equals h.FChargerId
                               where h.FId == item.FConnectorId
                               select new
                               {
                                   stationID = l.FId,
                                   station = l.FName,
                                   chargerID = t.FId,
                                   charger = t.FName,
                                   headerID = h.FId,
                                   header = h.FName,
                                   meter = h.FCurrentMeter,
                                   //image = l.FImage,
                                   logo = l.FLogo
                               };
                var charger = chargers.FirstOrDefault();

                // คำนวณความแตกต่างระหว่างวันที่
                TimeSpan timeDifference = (item.FEndTime ?? DateTime.UtcNow) - (item.FStartTime ?? DateTime.UtcNow);

                // แยกชั่วโมงและนาทีจาก TimeSpan
                int hours = timeDifference.Hours;
                int minutes = timeDifference.Minutes;


                translist.Add(new
                {
                    datetime = ConvertData.FormatThaiDate(item.FCreated),
                    stationname = charger.station,
                    headname = $"{charger.charger} {charger.header}",
                    meterrate = item.FPreMeter,
                    cost = item.FCost,
                    hour = hours,
                    minute = minutes,
                    //image = charger.image,
                    logo = charger.logo
                });
            }
            return Ok(new { data = translist });
        }

        /// <summary>
        /// Initiates a new payment transaction for electric vehicle charging.
        /// Creates transaction and payment records, then calls external payment gateway API.
        /// </summary>
        /// <param name="request">Transaction request containing station, charger, and payment details</param>
        /// <returns>Payment gateway response with QR code and order information</returns>
        [HttpPost("payment")]
        public async Task<IActionResult> GetTranslationsAsync([FromBody] TransactionRequestModel request)
        {
            var userID = User?.FindFirstValue(JwtRegisteredClaimNames.Sid);
            var api = _context.TblPaymentApis.FirstOrDefault();
            var station = _context.TblStations.FirstOrDefault(x => x.FId == Guid.Parse(request.Stationid));
            var user = _context.TblUsers.FirstOrDefault(x => x.FId == Guid.Parse(userID));
            var transaction = new Entities.TblTransaction()
            {
                FChargerId = Guid.Parse(request.Chargerid),
                //FCost = request.Amount,
                FCreated = DateTime.UtcNow,
                FMeterEnd = 0,
                FMeterStart = 0,
                FStationId = Guid.Parse(request.Stationid),
                FStatus = 'Y',
                FUserId = user.FId,
                FConnectorId = Guid.Parse(request.Headerid),
                FPreMeter = request.Meter
            };
            _context.TblTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            var referenceNo = ConvertData.ConvertYYMMDD(DateTime.UtcNow);
            var paymentcount = _context.TblPayments.Count(x => x.FPaymentCode.StartsWith(referenceNo));
            referenceNo = $"{referenceNo}{(paymentcount + 1).ToString("D6")}";
            var payment = new Entities.TblPayment()
            {
                FCreateby = user.FId,
                FCreated = DateTime.UtcNow,
                FNet = request.Amount,
                FPaymentAmount = request.Amount,
                FPaymentStatus = "Pending",
                FStatus = 'Y',
                FTransactionId = transaction.FId,
                FVat = 0,
                FPaymentCode = referenceNo,
            };
            _context.TblPayments.Add(payment);
            await _context.SaveChangesAsync();

            using (var httpClient = new HttpClient())
            {
                // สร้าง URL สำหรับการเรียก API
                var url = $"{api.FUrl}?" +
                    $"merchantID={api.FMerchantId}&" +
                    $"productDetail=ChargeMe&" +
                    $"customerEmail={station.FEmail}&" +
                    $"customerName={station.FName}&" +
                    $"total={request.Amount}&" +
                    $"referenceNo={referenceNo}";

                // ตั้งค่า Authorization Header
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", api.FToken);

                // สร้างคำขอ POST
                var response = await httpClient.PostAsync(url, null); // ไม่มี Body ในคำขอ

                // ตรวจสอบผลลัพธ์
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var _payment = JsonConvert.DeserializeObject<TransactionResponseModel>(jsonResponse); // แปลง JSON เป็น Object ตามต้องการ
                    if (_payment.status == "success")
                    {
                        payment.FQrImage = _payment.data.Image;
                        payment.FOrderNo = _payment.data.OrderNo;
                        payment.FOrderDatetime = Helper.ConvertData.ConvertChristianToBuddhistDateDate(_payment.data.Orderdatetime).ToUniversalTime();
                        payment.FExpireDate = Helper.ConvertData.ConvertChristianToBuddhistDateDate(_payment.data.Expiredate).ToUniversalTime();
                        _context.TblPayments.Update(payment);
                        await _context.SaveChangesAsync();

                        _payment.data.Fid = payment.FId.ToString();
                        _payment.data.StationName = station.FName;
                        _payment.data.Detail = "เติมไฟอีวี";
                        return Ok(new { data = _payment, transdata = transaction });
                    }
                    else
                    {
                        transaction.FTransactionStatus = "Cancel";
                        _context.TblTransactions.Update(transaction);
                        await _context.SaveChangesAsync();

                        payment.FPaymentStatus = "Cancel";
                        _context.TblPayments.Update(payment);
                        await _context.SaveChangesAsync();
                        return Ok(new TransactionResponseModel() { status = _payment.status });
                    }
                }
                else
                {
                    transaction.FTransactionStatus = "Cancel";
                    _context.TblTransactions.Update(transaction);
                    await _context.SaveChangesAsync();

                    payment.FPaymentStatus = "Cancel";
                    _context.TblPayments.Update(payment);
                    await _context.SaveChangesAsync();

                    // จัดการกับข้อผิดพลาด
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error: {errorMessage}");
                }
            }

            return Ok(new TransactionResponseModel());
        }

        /// <summary>
        /// Checks the status of a pending payment transaction.
        /// Retrieves cached data for real-time payment status updates.
        /// </summary>
        /// <param name="request">Request containing the transaction ID to check</param>
        /// <returns>Cached transaction and payment status data</returns>
        [HttpPost("status")]
        public async Task<IActionResult> GetStatus([FromBody] TransactionCheckRequestModel request)
        {
            var cachdata = ((dynamic)await _cachService.GetDataAsync(Guid.Parse(request.Fid)));

            return Ok(new { data = cachdata.data, transdata = cachdata.transdata });
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartTransaction([FromBody] StartTransactionRequest request)
        {
            try
            {
                var payment = await _context.TblPayments.FirstOrDefaultAsync(x => x.FPaymentCode == request.RefNo);
                var trans = await _context.TblTransactions.FirstOrDefaultAsync(x => x.FId == payment.FTransactionId);
                var charger = await _context.TblChargers.FirstOrDefaultAsync(x => x.FId == trans.FChargerId);
                var station = await _context.TblStations.FirstOrDefaultAsync(x => x.FId == trans.FStationId);
                var connector = await _context.TblConnectorStatuses.FirstOrDefaultAsync(x => x.FId == trans.FConnectorId);
                if (charger != null && station != null)
                {
                    await _ocppService.StartChargingSession(trans.FChargerId, charger.FCode, (int)connector.FConnectorId, station.FRfid);
                }

                return Ok(new { message = "Transaction start successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.ToString() });
            }
        }

        [HttpPost("connector-status")]
        public async Task<IActionResult> CheckConnectorStatus([FromBody] CheckConnectorRequest request)
        {
            try
            {
                var connector = await _context.TblConnectorStatuses.FirstOrDefaultAsync(x => x.FCode == request.ConnectorCode);
                if (connector != null && connector != null)
                {
                    var charger = await _context.TblChargers.FirstOrDefaultAsync(x => x.FId == connector.FChargerId);

                    var responseBody = await _ocppService.CheckStatus();

                    var chargePoints = JsonConvert.DeserializeObject<List<ChargePoint>>(responseBody);
                    if (chargePoints != null && charger != null)
                    {
                        var chargePoint = chargePoints.FirstOrDefault(cp => cp.Id == charger.FCode);

                        if (chargePoint == null)
                            return StatusCode(500, new { message = "หัวชาร์จไม่พร้อมทำงาน" });

                        string connectorKey = connector.FConnectorId.ToString();
                        if (chargePoint.OnlineConnectors != null &&
                            chargePoint.OnlineConnectors.TryGetValue(connectorKey, out var connectorCheck))
                        {
                            if (connectorCheck.Status == 1)
                                return Ok(new { message = "Available" });
                            else if (connectorCheck.Status == 5)
                                return Ok(new { message = "Preparing" });
                            else if (connectorCheck.Status == 6)
                            {
                                if (request.IgnoreCharging)
                                    return Ok(new { message = "Charging" });
                                else
                                    return StatusCode(500, new { message = "หัวชาร์จนี้กำลังใช้งานอยู่" });
                            }
                        }
                    }
                    return StatusCode(500, new { message = "หัวชาร์จไม่พร้อมทำงาน" });
                }
                else
                    return StatusCode(500, new { message = "ไม่พบหัวชาร์จนี้ กรูณาระบุใหม่อีกครั้ง" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.ToString() });
            }
        }

        [HttpPost("stop")]
        public async Task<IActionResult> StopTransaction([FromBody] StopTransactionRequest request)
        {
            try
            {
                var payment = await _context.TblPayments.FirstOrDefaultAsync(x => x.FPaymentCode == request.RefNo);
                if (payment == null)
                    return StatusCode(500, new { message = "RefNo not found" });

                var trans = await _context.TblTransactions.FirstOrDefaultAsync(x => x.FId == payment.FTransactionId);
                if (trans == null)
                    return StatusCode(500, new { message = "TransactionId not found" });

                var connector = await _context.TblConnectorStatuses.FirstOrDefaultAsync(x => x.FId == trans.FConnectorId);
                if (connector == null)
                    return StatusCode(500, new { message = "Connector not found" });

                var charger = await _context.TblChargers.FirstOrDefaultAsync(x => x.FId == trans.FChargerId);
                var station = await _context.TblStations.FirstOrDefaultAsync(x => x.FId == trans.FStationId);

                // trans.FMeterStart = Convert.ToDecimal(meterStart);
                trans.FMeterEnd = Convert.ToDecimal(request.MeterEnd);
                trans.FTransactionStatus = "Finishing";
                trans.FUpdated = DateTime.UtcNow;
                trans.FEndTime = DateTime.UtcNow;
                trans.FEndResult = "EmergencyStop";
                _context.TblTransactions.Update(trans);
                _ = await _context.SaveChangesAsync();

                connector.FStateOfCharge = 0;
                connector.FCurrentChargeKw = 0;
                connector.FCurrentMeter = 0;
                connector.FCurrentMeterTime = DateTime.UtcNow;

                _context.TblConnectorStatuses.Update(connector);
                _ = await _context.SaveChangesAsync();

                if (trans.FPreMeter > request.MeterEnd)
                {
                    try
                    {
                        var userID = User?.FindFirstValue(JwtRegisteredClaimNames.Sid);
                        if (userID != null)
                        {
                            var user = await _context.TblUsers.FirstOrDefaultAsync(x => x.FId == Guid.Parse(userID));
                            if (user != null)
                            {
                                user.FBalanceKwh += (trans.FPreMeter - request.MeterEnd);
                                _context.TblUsers.Update(user);
                                _ = await _context.SaveChangesAsync();
                            }
                        }
                    }
                    catch
                    {

                    }
                }

                _cachService.ClearData(trans.FId);

                if (charger != null && station != null)
                {
                    await _ocppService.StopTransaction(trans.FChargerId, charger.FCode, (int)connector.FConnectorId, station.FRfid);
                }

                return Ok(new { message = "Transaction stop successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.ToString() });
            }
        }
    }

    public class ConnectorInfo
    {
        public int Status { get; set; }
        public decimal? ChargeRateKW { get; set; }
        public decimal? MeterKWH { get; set; }
        public decimal? SoC { get; set; }
    }

    public class ChargePoint
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Protocol { get; set; }
        public int? Heartbeat { get; set; }
        public Dictionary<string, ConnectorInfo> OnlineConnectors { get; set; }
        public string WebSocketStatus { get; set; }
    }
}

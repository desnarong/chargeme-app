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
        
        /// <summary>
        /// Initializes a new instance of the TransactionController.
        /// </summary>
        /// <param name="context">The database context for data access</param>
        /// <param name="cachService">The memory cache service for temporary data storage</param>
        public TransactionController(NpgsqlDbContext context, MemoryCacheService cachService)
        {
            _context = context;
            _cachService = cachService;
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
    }
}

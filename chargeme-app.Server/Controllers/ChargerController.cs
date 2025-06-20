using chargeme_app.Server.DataContext;
using chargeme_app.Server.Entities;
using chargeme_app.Server.Helper;
using chargeme_app.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Security.Claims;
using static System.Collections.Specialized.BitVector32;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace chargeme_app.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ChargerController : ControllerBase
    {
        private readonly NpgsqlDbContext _context;

        public ChargerController(NpgsqlDbContext context)
        {
            _context = context;
        }
        [HttpGet("chargers")]
        public IActionResult GetChargers()
        {
            var chargers = _context.TblChargers.Where(x => x.FStatus == 'Y').ToList();
            return Ok(new { data = chargers });
        }

        [HttpPost("check")]
        public IActionResult GetCharger([FromBody] ChargerRequestModel request)
        {
            var userID = User?.FindFirstValue(JwtRegisteredClaimNames.Sid);
            var chargers = from t in _context.TblChargers
                               join l in _context.TblStations on t.FStationId equals l.FId
                               join h in _context.TblConnectorStatuses on t.FId equals h.FChargerId
                               join u in _context.TblChargerUnits on l.FUnitId equals u.FId
                               join c in _context.TblCompanies on l.FCompanyId equals c.FId
                               where h.FCode == request.Chargeid
                               select new
                               {
                                   stationID = l.FId,
                                   station = l.FName,
                                   company = c.FName,
                                   company_addr = $"{c.FAddress} {c.FCity} {c.FProvince} {c.FPostcode} โทร.{c.FOffice}",
                                   station_addr = $"{l.FAddress} {l.FCity} {l.FProvince} {l.FPostcode} โทร.{l.FOffice}",
                                   chargerID = t.FId,
                                   charger = t.FName,
                                   headerID = h.FId,
                                   header = h.FName,
                                   currentStatus = h.FCurrentStatus,
                                   minimumAmount = l.FMinimumAmount,
                                   unit = u.FName,
                                   tax = l.FVatnumber,
                                   code = h.FCode,
                                   image = l.FImage,
                                   logo = c.FLogo
                               };
            var charger = chargers.FirstOrDefault();
            var paydata = new TransactionResponseModel() { status = "success", data = null };
            var transata = new TblTransaction();
            if (charger != null)
            {
                var pay = _context.TblPayments.Where(x => x.FCreateby == Guid.Parse(userID) && x.FPaymentStatus.ToLower() != "cancel").OrderByDescending(x => x.FPaymentCode).FirstOrDefault();
                
                if(pay != null)
                {
                    transata = _context.TblTransactions.FirstOrDefault(x => x.FId == pay.FTransactionId && x.FTransactionStatus.ToLower() != "cancel");
                    try
                    {
                        if(transata != null)
                        {
                            paydata = new TransactionResponseModel()
                            {
                                status = "success",
                                data = new PromptpayDataModel()
                                {
                                    Expiredate = Helper.ConvertData.ConvertBuddhistDateToChristianDate(pay.FExpireDate.Value),
                                    Fid = pay.FId.ToString(),
                                    Detail = "เติมไฟอีวี",
                                    Image = pay.FQrImage,
                                    Orderdatetime = Helper.ConvertData.ConvertBuddhistDateToChristianDate(pay.FOrderDatetime.Value),
                                    OrderNo = pay.FOrderNo,
                                    ReferenceNo = pay.FOrderNo,
                                    StationName = charger.station,
                                    Total = pay.FPaymentAmount ?? 0,
                                    Status = pay.FPaymentStatus
                                }
                            };
                        }
                    }
                    catch(Exception err)
                    {
                        pay.FPaymentStatus = "cancel";
                        _context.TblPayments.Update(pay);
                        _context.SaveChanges();

                        transata.FTransactionStatus = "cancel";
                        _context.TblTransactions.Update(transata);
                        _context.SaveChanges();

                        transata = null;
                        paydata = null;
                    }
                }
                
            }
            return Ok(new { data = charger, pay = paydata, trans = transata });
        }
        [HttpPost("priceshows")]
        public IActionResult GetPriceShows([FromBody] ChargerPriceShowRequestModel request)
        {
            var prices = from p in _context.TblChargerPriceShows
                         join s in _context.TblStations on p.FStationId equals s.FId
                         join u in _context.TblChargerUnits on s.FUnitId equals u.FId
                         where p.FStationId == Guid.Parse(request.Stationid)
                         orderby p.FValue
                         select new
                         {
                             price = p.FValue,
                             unit = u.FName,
                             hour = p.FText
                         };

            return Ok(new { data = prices.ToList() });
        }
        [HttpPost("cal")]
        public IActionResult GetCalPrice([FromBody] ChargerPriceCalRequestModel request)
        {
            DateTime currentTime = DateTime.Now; // เวลา ณ ปัจจุบัน
            var station = _context.TblStations.First(x => x.FId == Guid.Parse(request.Stationid));
            decimal rate;
            if (HelpApp.IsOnPeak(currentTime))
            {
                rate = station.FOnpeak ?? 0; // อัตรา On-Peak
            }
            else
            {
                rate = station.FOffpeak ?? 0; // อัตรา Off-Peak
            }

            if (station.FChagerType == 1)
            {
                
                return Ok(new { data = HelpApp.CalculateUnits(request.Amount, rate).ToString("F2") });
            }
            else
            {
                string[] timeParts = request.Hour.Split(':');

                decimal hours = decimal.Parse(timeParts[0]);
                decimal minutes = decimal.Parse(timeParts[1]);
                hours += (minutes / 60);
                rate = HelpApp.CalculatePrice(request.Amount, (rate * hours));
                return Ok(new { data = rate.ToString("F2") });
            }
        }
    }
}

using csms.Entities;
using csms.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using QuestPDF.Fluent;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace csms.Controllers
{
    public class TransactionController : Controller
    {
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(ILogger<TransactionController> logger)
        {
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult GetTransactionTable(string station, string chargepoint, string startdate, string enddate)
        {
            var eus = new System.Globalization.CultureInfo("en-US");
            string draw = Request.Form["draw"][0];
            string order = Request.Form["order[0][column]"][0];
            string orderDir = Request.Form["order[0][dir]"][0];
            int startRec = Convert.ToInt32(Request.Form["start"][0]);
            int pageSize = Convert.ToInt32(Request.Form["length"][0]);

            var data = TransactionModel.GetTransactionDatas();

            if (!station.Equals("0"))
            {
                data = TransactionModel.GetTransactionDatas(Guid.Parse(station));
            }
            if (!chargepoint.Equals("0"))
            {
                var chargepoints = chargepoint.Split('_');
                data = data.Where(x => x.ChargerId == Guid.Parse(chargepoints[0]) && x.ConnectorNo == int.Parse(chargepoints[1])).ToList();
            }
            //if(chargetag != null)
            //{
            //    if (!chargetag.Equals("0"))
            //    {
            //        data = data.Where(x => x.StartTagId == chargetag).ToList();
            //    }
            //}
            
            if (!string.IsNullOrEmpty(startdate))
            {
                data = data.Where(x => x.StartTime > DateTime.ParseExact(startdate, "dd/MM/yyyy", eus)).ToList();
            }
            if (!string.IsNullOrEmpty(enddate))
            {
                data = data.Where(x => x.StartTime < DateTime.ParseExact(enddate, "dd/MM/yyyy", eus)).ToList();
            }

            foreach (var item in data)
            {
                try
                {
                    var timespan = item.StopTime - item.StartTime;
                    if (timespan.Value.Hours > 0)
                    {
                        var result = string.Format("{0:D1} ชม {1:D1} นาที", timespan.Value.Hours, timespan.Value.Minutes);
                        item.UsedTime = result;

                    }
                    else
                    {
                        var result = string.Format("{0:D1} นาที", timespan.Value.Minutes);
                        item.UsedTime = result;
                    }
                }
                catch(Exception ex)
                {

                }

            }

            switch (order)
            {
                case "0":
                    data = orderDir.StartsWith("asc") ? data.OrderBy(x => x.ChargerId).ToList() : data.OrderByDescending(x => x.ChargerId).ToList();
                    break;
                case "1":
                    data = orderDir.StartsWith("asc") ? data.OrderBy(x => x.ConnectorId).ToList() : data.OrderByDescending(x => x.ConnectorId).ToList();
                    break;
                case "2":
                    data = orderDir.StartsWith("asc") ? data.OrderBy(x => x.StartTagId).ToList() : data.OrderByDescending(x => x.StartTagId).ToList();
                    break;
                case "3":
                    data = orderDir.StartsWith("asc") ? data.OrderBy(x => x.PlateNo).ToList() : data.OrderByDescending(x => x.PlateNo).ToList();
                    break;
                case "4":
                    data = orderDir.StartsWith("asc") ? data.OrderBy(x => x.StartDateTime).ToList() : data.OrderByDescending(x => x.StartDateTime).ToList();
                    break;
                case "5":
                    data = orderDir.StartsWith("asc") ? data.OrderBy(x => x.UsedTime).ToList() : data.OrderByDescending(x => x.UsedTime).ToList();
                    break;
                case "6":
                    data = orderDir.StartsWith("asc") ? data.OrderBy(x => x.ChargeSum).ToList() : data.OrderByDescending(x => x.ChargeSum).ToList();
                    break;
                case "7":
                    data = orderDir.StartsWith("asc") ? data.OrderBy(x => x.TransactionId).ToList() : data.OrderByDescending(x => x.TransactionId).ToList();
                    break;
                case "8":
                    data = orderDir.StartsWith("asc") ? data.OrderBy(x => x.StopReason).ToList() : data.OrderByDescending(x => x.StopReason).ToList();
                    break;
            }

            int totalRecords = data.Count;
            if (pageSize > -1)
            {
                data = data.Skip(startRec).Take(pageSize).ToList();
            }
            else
            {
                data = data.Skip(startRec).ToList();
            }

            return new JsonResult(new { draw = draw, iTotalRecords = totalRecords, iTotalDisplayRecords = totalRecords, data = data });
        }
        public IActionResult GetStationSelect()
        {
            var userInfo = GetUserInfo();
            if (userInfo.FCompanyId == Guid.Empty)
            {
                var data = StationInfoModel.GetStationInfoes();
                data = data.OrderBy(x => x.Name).ToList();
                return new JsonResult(new { data = data });
            }
            else
            {
                var data = StationInfoModel.GetStationInfoes(userInfo.FCompanyId);
                data = data.OrderBy(x => x.Name).ToList();
                return new JsonResult(new { data = data });
            }
        }
        public IActionResult GetChargePointSelect()
        {
            var userInfo = GetUserInfo();
            if (userInfo.FCompanyId == Guid.Empty)
            {
                var data = ChargePointModel.GetConnectorStatusViews();
                data = data.OrderBy(x => x.ShortName).ThenBy(x => x.ConnectorId).ToList();
                return new JsonResult(new { data = data });
            }
            else
            {
                var data = ChargePointModel.GetConnectorStatusViews(userInfo.FCompanyId);
                data = data.OrderBy(x => x.ShortName).ThenBy(x => x.ConnectorId).ToList();
                return new JsonResult(new { data = data });
            }  
        }
        public IActionResult GetChargeTagSelect()
        {
            var data = ChargeTagModel.GetChargeTags();
            data = data.OrderBy(x => x.FName).ToList();
            return new JsonResult(new { data = data });
        }
        public IActionResult GetChargeTagPlateNo(string chargetagid)
        {
            var data = ChargeTagModel.GetChargeTag(Guid.Parse(chargetagid));
            return new JsonResult(new { data = data });
        }
        public IActionResult Print(string chargepoint, string chargetag, string startdate, string enddate)
        {
            var eus = new System.Globalization.CultureInfo("en-US");
            var th = new System.Globalization.CultureInfo("th-TH");
            var data = TransactionModel.GetTransactionDatas();
            DateTime _startdate = DateTime.Now, _enddate = DateTime.Now;
            if (!chargepoint.Equals("0"))
            {
                var chargepoints = chargepoint.Split('_');
                data = data.Where(x => x.ChargerId == Guid.Parse(chargepoints[0]) && x.ConnectorId == Guid.Parse(chargepoints[1])).ToList();
            }
            if (!chargetag.Equals("0"))
            {
                data = data.Where(x => x.StartTagId == chargetag).ToList();
            }
            if (!string.IsNullOrEmpty(startdate))
            {
                _startdate = DateTime.ParseExact(startdate, "dd/MM/yyyy", eus);
                data = data.Where(x => x.StartTime > _startdate).ToList();
            }
            if (!string.IsNullOrEmpty(enddate))
            {
                _enddate = DateTime.ParseExact(enddate, "dd/MM/yyyy", eus);
                data = data.Where(x => x.StartTime < _enddate).ToList();
            }

            foreach (var item in data)
            {
                var timespan = item.StopTime - item.StartTime;
                if (timespan.Value.Hours > 0)
                {
                    var result = $"{timespan.Value.Hours} ชม {timespan.Value.Minutes} นาที ";
                    item.UsedTime = result;

                }
                else
                {
                    var result = $"{timespan.Value.Minutes} นาที ";
                    item.UsedTime = result;
                }

            }
            var userInfo = GetUserInfo();
            var companyinfo = CompanyInfoModel.GetCompanyInfo(userInfo.FCompanyId);
            TransactionReport report = new TransactionReport(data, companyinfo, _startdate.ToString("dd/MM/yyyy", th), _enddate.ToString("dd/MM/yyyy", th));
            var pdf = report.GeneratePdf();
            return new FileContentResult(pdf, "application/pdf");
        }

        private TblUser GetUserInfo()
        {
            var userdata = HttpContext.Session.GetString("UserData");
            return JsonConvert.DeserializeObject<TblUser>(userdata) ?? null;
        }
    }
}

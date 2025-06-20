using csms.Entities;
using csms.Helpers;
using csms.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Drawing;
using System.Globalization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace csms.Controllers
{
    public class StationInfoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Holiday(string id)
        {
            Entities.TblStation companyInfoData = StationInfoModel.GetStationInfo(Guid.Parse(id));
            return View(companyInfoData);
        }
        public IActionResult GetStationInfoTable()
        {
            string draw = Request.Form["draw"][0];
            string order = Request.Form["order[0][column]"][0];
            string orderDir = Request.Form["order[0][dir]"][0];
            int startRec = Convert.ToInt32(Request.Form["start"][0]);
            int pageSize = Convert.ToInt32(Request.Form["length"][0]);
            int totalRecords = 0;

            var userInfo = JsonConvert.DeserializeObject<TblUser>(HttpContext.Session.GetString("UserData"));
            var model = new List<StationInfoData>();
            if (userInfo.FCompanyId == Guid.Empty)
                model = StationInfoModel.GetStationInfoes();
            else
                model = StationInfoModel.GetStationInfoes(userInfo.FCompanyId ?? Guid.Empty);

            switch (order)
            {
                case "0":
                    model = orderDir == "asc" ? model.OrderBy(x => x.Id).ToList() : model.OrderByDescending(x => x.Id).ToList();
                    break;
                case "1":
                    model = orderDir == "asc" ? model.OrderBy(x => x.Name).ToList() : model.OrderByDescending(x => x.Name).ToList();
                    break;
            }
            totalRecords = model.Count;
            if (pageSize > -1)
            {
                model = model.Skip(startRec).Take(pageSize).ToList();
            }
            else
            {
                model = model.Skip(startRec).ToList();
            }

            foreach (var station in model)
            {
                station.ActionHoliday = $"<a href='{Url.Action("Holiday", "StationInfo", new { id = station.Id })}' class='btn btn-clean btn-icon btn-icon-md' title='View'><i class='fa-solid fa-calendar-days'></i></a>";
            }

            return new JsonResult(new { draw = draw, iTotalRecords = totalRecords, iTotalDisplayRecords = totalRecords, data = model });
        }
        public IActionResult GetHolidayTable(string stationid)
        {
            string draw = Request.Form["draw"][0];
            string order = Request.Form["order[0][column]"][0];
            string orderDir = Request.Form["order[0][dir]"][0];
            int startRec = Convert.ToInt32(Request.Form["start"][0]);
            int pageSize = Convert.ToInt32(Request.Form["length"][0]);
            

            var holidays = StationInfoModel.GetHolidayDatas(Guid.Parse(stationid)).OrderBy(x => x.Day).ToList();
            int totalRecords = holidays.Count;
            return new JsonResult(new { draw = draw, iTotalRecords = totalRecords, iTotalDisplayRecords = totalRecords, data = holidays });
        }
        public IActionResult GetHourRateTable(string station)
        {
            string draw = Request.Form["draw"][0];
            //string order = Request.Form["order[0][column]"][0];
            //string orderDir = Request.Form["order[0][dir]"][0];
            //int startRec = Convert.ToInt32(Request.Form["start"][0]);
            //int pageSize = Convert.ToInt32(Request.Form["length"][0]);
            int totalRecords = 0;

            var model = StationInfoModel.GetChargerPriceShows(Guid.Parse(station));

            if(model.Count<=0)
            {
                model.Add(new ChargerPriceShowData() { Id = Guid.Empty, Text = "1", Value = "0", UnitId = Guid.Parse("ec5e0175-70b5-4e9c-8e33-72c35c5f7e0e"), ActionValue = $"<input id='{Guid.Empty}' value='0' data-text='1:00' class='form-control'>" });
                model.Add(new ChargerPriceShowData() { Id = Guid.Empty, Text = "1:30", Value = "0", UnitId = Guid.Parse("ec5e0175-70b5-4e9c-8e33-72c35c5f7e0e"), ActionValue = $"<input id='{Guid.Empty}' value='0' data-text='1:30' class='form-control'>" });
                model.Add(new ChargerPriceShowData() { Id = Guid.Empty, Text = "2", Value = "0", UnitId = Guid.Parse("ec5e0175-70b5-4e9c-8e33-72c35c5f7e0e"), ActionValue = $"<input id='{Guid.Empty}' value='0' data-text='2:00' class='form-control'>" });
                model.Add(new ChargerPriceShowData() { Id = Guid.Empty, Text = "2:30", Value = "0", UnitId = Guid.Parse("ec5e0175-70b5-4e9c-8e33-72c35c5f7e0e"), ActionValue = $"<input id='{Guid.Empty}' value='0' data-text='2:30' class='form-control'>" });
                model.Add(new ChargerPriceShowData() { Id = Guid.Empty, Text = "3", Value = "0", UnitId = Guid.Parse("ec5e0175-70b5-4e9c-8e33-72c35c5f7e0e"), ActionValue = $"<input id='{Guid.Empty}' value='0' data-text='3:00' class='form-control'>" });
                model.Add(new ChargerPriceShowData() { Id = Guid.Empty, Text = "3:30", Value = "0", UnitId = Guid.Parse("ec5e0175-70b5-4e9c-8e33-72c35c5f7e0e"), ActionValue = $"<input id='{Guid.Empty}' value='0' data-text='3:30' class='form-control'>" });
                model.Add(new ChargerPriceShowData() { Id = Guid.Empty, Text = "4", Value = "0", UnitId = Guid.Parse("ec5e0175-70b5-4e9c-8e33-72c35c5f7e0e"), ActionValue = $"<input id='{Guid.Empty}' value='0' data-text='4:00' class='form-control'>" });
                model.Add(new ChargerPriceShowData() { Id = Guid.Empty, Text = "5", Value = "0", UnitId = Guid.Parse("ec5e0175-70b5-4e9c-8e33-72c35c5f7e0e"), ActionValue = $"<input id='{Guid.Empty}' value='0' data-text='5:00' class='form-control'>" });
                model.Add(new ChargerPriceShowData() { Id = Guid.Empty, Text = "6", Value = "0", UnitId = Guid.Parse("ec5e0175-70b5-4e9c-8e33-72c35c5f7e0e"), ActionValue = $"<input id='{Guid.Empty}' value='0' data-text='6:00' class='form-control'>" });
                model.Add(new ChargerPriceShowData() { Id = Guid.Empty, Text = "7", Value = "0", UnitId = Guid.Parse("ec5e0175-70b5-4e9c-8e33-72c35c5f7e0e"), ActionValue = $"<input id='{Guid.Empty}' value='0' data-text='7:00' class='form-control'>" });
                model.Add(new ChargerPriceShowData() { Id = Guid.Empty, Text = "8", Value = "0", UnitId = Guid.Parse("ec5e0175-70b5-4e9c-8e33-72c35c5f7e0e"), ActionValue = $"<input id='{Guid.Empty}' value='0' data-text='8:00' class='form-control'>" });
                model.Add(new ChargerPriceShowData() { Id = Guid.Empty, Text = "9", Value = "0", UnitId = Guid.Parse("ec5e0175-70b5-4e9c-8e33-72c35c5f7e0e"), ActionValue = $"<input id='{Guid.Empty}' value='0' data-text='9:00' class='form-control'>" });
            }

            totalRecords = model.Count;
            //if (pageSize > -1)
            //{
            //    model = model.Skip(startRec).Take(pageSize).ToList();
            //}
            //else
            //{
            //    model = model.Skip(startRec).ToList();
            //}

            return new JsonResult(new { draw = draw, iTotalRecords = totalRecords, iTotalDisplayRecords = totalRecords, data = model });
        }
        public IActionResult CreateStation()
        {
            byte[]? Images = null;
            byte[]? Logo = null;
            string code = Request.Form["code"];
            string name = Request.Form["name"];
            string address = Request.Form["address"];
            string city = Request.Form["city"];
            string country = Request.Form["country"];
            string postcode = Request.Form["postcode"];
            string tel = Request.Form["tel"];
            string rfid = Request.Form["rfid"];
            string company = Request.Form["company"];
            string chargetype = Request.Form["chargertype"];
            string latitude = Request.Form["lat"];
            string longitude = Request.Form["long"];

            if (Request.Form.Files.Count > 0)
            {
                IFormFileCollection files = Request.Form.Files;

                var imageFile = files["image"]; // ดึงไฟล์จาก Key "image"
                var logoFile = files["logo"];   // ดึงไฟล์จาก Key "logo"
                if (imageFile != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        imageFile.CopyTo(ms);
                        using (var img = Image.FromStream(ms))
                        {
                            // TODO: ResizeImage(img, 100, 100);
                            var newimage = ImageHelper.ResizeImage(img, img.Width / 2, img.Height / 2);
                            Images = ImageHelper.CopyImageToByteArray(newimage);
                        }
                    }
                }
                if (logoFile != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        logoFile.CopyTo(ms);
                        using (var img = Image.FromStream(ms))
                        {
                            // TODO: ResizeImage(img, 100, 100);
                            var newimage = ImageHelper.ResizeImage(img, img.Width / 2, img.Height / 2);
                            Logo = ImageHelper.CopyImageToByteArray(newimage);
                        }
                    }
                }
            }

            var station = new Entities.TblStation();
            station.FName = name;
            station.FAddress = address;
            station.FCity = city;
            station.FProvince = country;
            station.FPostcode = postcode;
            station.FOffice = tel;
            station.FRfid = rfid;
            station.FCode = code;
            station.FCompanyId = Guid.Parse(company);
            station.FChagerType = int.Parse(chargetype);
            station.FLatitude = long.Parse(latitude);
            station.FLongitude = long.Parse(longitude);

            if (Images != null)
                station.FImage = Images;

            if (Logo != null)
                station.FLogo = Logo;

            StationInfoModel.Create(station);

            return Json("success");
        }
        public IActionResult UpadateStation()
        {
            byte[]? Images = null;
            byte[]? Logo = null;
            string id = Request.Form["id"];
            string name = Request.Form["name"];
            string address = Request.Form["address"];
            string city = Request.Form["city"];
            string country = Request.Form["country"];
            string postcode = Request.Form["postcode"];
            string tel = Request.Form["tel"];
            string rfid = Request.Form["rfid"];
            string company = Request.Form["company"];
            string chargetype = Request.Form["chargertype"];
            string latitude = Request.Form["lat"];
            string longitude = Request.Form["long"];

            if (Request.Form.Files.Count > 0)
            {
                IFormFileCollection files = Request.Form.Files;

                var imageFile = files["image"]; // ดึงไฟล์จาก Key "image"
                var logoFile = files["logo"];   // ดึงไฟล์จาก Key "logo"
                if (imageFile != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        imageFile.CopyTo(ms);
                        using (var img = Image.FromStream(ms))
                        {
                            // TODO: ResizeImage(img, 100, 100);
                            var newimage = ImageHelper.ResizeImage(img, img.Width / 2, img.Height / 2);
                            Images = ImageHelper.CopyImageToByteArray(newimage);
                        }
                    }
                }
                if (logoFile != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        logoFile.CopyTo(ms);
                        using (var img = Image.FromStream(ms))
                        {
                            // TODO: ResizeImage(img, 100, 100);
                            var newimage = ImageHelper.ResizeImage(img, img.Width / 2, img.Height / 2);
                            Logo = ImageHelper.CopyImageToByteArray(newimage);
                        }
                    }
                }
            }

            var station = StationInfoModel.GetStationInfo(Guid.Parse(id));
            station.FName = name;
            station.FAddress = address;
            station.FCity = city;
            station.FProvince = country;
            station.FPostcode = postcode;
            station.FOffice = tel;
            station.FRfid = rfid;
            station.FCompanyId = Guid.Parse(company);
            station.FChagerType = int.Parse(chargetype);
            station.FLatitude = long.Parse(latitude);
            station.FLongitude = long.Parse(longitude);
            
            if (Images != null)
                station.FImage = Images;
            if (Logo != null)
                station.FLogo = Logo;
            StationInfoModel.Update(station);

            return Json("success");
        }
        public IActionResult DeleteStation(string id)
        {
            var station = StationInfoModel.GetStationInfo(Guid.Parse(id));
            StationInfoModel.Delete(station);

            return Json("success");
        }
        public IActionResult UpadateElectricityRate()
        {
            string id = Request.Form["id"];
            string onpeak = Request.Form["onpeak"];
            string offpeak = Request.Form["offpeak"];

            var station = StationInfoModel.GetStationInfo(Guid.Parse(id));

            station.FOnpeak = Convert.ToDecimal(onpeak);
            station.FOffpeak = Convert.ToDecimal(offpeak);

            StationInfoModel.Update(station);

            return Json("success");
        }

        public IActionResult CreateHoliday()
        {
            string companyid = Request.Form["companyid"];
            string date = Request.Form["date"];
            string name = Request.Form["name"];

            StationInfoModel.Create(new Entities.TblHoliday()
            {
                FStationId = Guid.Parse(companyid),
                FDay = DateTime.ParseExact(date, "dd/MM/yyyy", new System.Globalization.CultureInfo("en-US")),
                FName = name
            });

            return Json("success");
        }
        public IActionResult UpdateHoliday()
        {
            string id = Request.Form["id"];
            string date = Request.Form["date"];
            string name = Request.Form["name"];

            var holiday = StationInfoModel.GetHoliday(Guid.Parse(id));
            holiday.FDay = DateTime.ParseExact(date, "dd/MM/yyyy", new System.Globalization.CultureInfo("en-US"));
            holiday.FName = name;

            StationInfoModel.Update(holiday);

            return Json("success");
        }
        public IActionResult DeleteHoliday(string id)
        {
            var holiday = StationInfoModel.GetHoliday(Guid.Parse(id));
            StationInfoModel.Delete(holiday);
            return Json("success");
        }

        public IActionResult UpadateHourRate()
        {
            // สร้าง List เพื่อเก็บข้อมูล
            var chargerPriceShowDataList = new List<ChargerPriceShowData>();

            string stationid = Request.Form["id"];
            //var inputs = Request.Form.Keys.Where(x => x.Contains("input")).ToList();
            var formData = Request.Form;
            // ตัวอย่างการดึงค่าจาก FormData
            for (int i = 1; i < formData.Count; i++)
            {
                var key = formData.Keys.ElementAt(i);
                var value = formData[key];

                // ตรวจสอบ key และดึงค่าตามที่ต้องการ
                if (key.StartsWith("inputs["))
                {
                    // ดึง index จาก key
                    var index = key.Split('[')[1].Split(']')[0];

                    // ดึง field จาก key
                    var field = key.Split('[')[2].Split(']')[0];

                    // ตรวจสอบว่า List มีข้อมูลสำหรับ index นี้หรือยัง
                    if (chargerPriceShowDataList.Count <= int.Parse(index))
                    {
                        chargerPriceShowDataList.Add(new ChargerPriceShowData());
                    }

                    // ตรวจสอบ field และดึงค่าตามที่ต้องการ
                    switch (field)
                    {
                        case "id":
                            chargerPriceShowDataList[int.Parse(index)].Id = Guid.Parse(value);
                            break;
                        case "text":
                            chargerPriceShowDataList[int.Parse(index)].Text = value;
                            break;
                        case "value":
                            chargerPriceShowDataList[int.Parse(index)].Value = value;
                            break;
                    }
                }
            }

            var unitId = Guid.Parse("ec5e0175-70b5-4e9c-8e33-72c35c5f7e0e");
            foreach (var data in chargerPriceShowDataList)
            {
                if(data.Id == Guid.Empty)
                {
                    StationInfoModel.Create(new TblChargerPriceShow()
                    {
                        FChargerUnitId = unitId,
                        FCreated = DateTime.UtcNow,
                        FStationId = Guid.Parse(stationid),
                        FText = data.Text,
                        FValue = decimal.Parse(data.Value)
                    });
                }
                else
                {
                    var model = StationInfoModel.GetChargerPrice(data.Id);
                    model.FValue = decimal.Parse(data.Value);
                    model.FText = data.Text;
                    model.FUpdated = DateTime.UtcNow;
                    StationInfoModel.Update(model);
                }
            }
            //StationInfoModel.Create(new Entities.TblHoliday()
            //{
            //    FStationId = Guid.Parse(companyid),
            //    FDay = DateTime.ParseExact(date, "dd/MM/yyyy", new System.Globalization.CultureInfo("en-US")),
            //    FName = name
            //});

            return Json("success");
        }
    }
}

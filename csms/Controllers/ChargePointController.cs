using csms.Helpers;
using csms.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Drawing;
using System.IO;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace csms.Controllers
{
    public class ChargePointController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult GetChargePointTable()
        {
            string draw = Request.Form["draw"][0];
            string order = Request.Form["order[0][column]"][0];
            string orderDir = Request.Form["order[0][dir]"][0];
            int startRec = Convert.ToInt32(Request.Form["start"][0]);
            int pageSize = Convert.ToInt32(Request.Form["length"][0]);

            var model = ChargePointModel.GetChargePointDatas();

            switch (order)
            {
                case "0":
                    model = orderDir == "asc" ? model.OrderBy(x => x.ChargerId).ToList() : model.OrderByDescending(x => x.ChargerId).ToList();
                    break;
            }


            int totalRecords = model.Count;
            if (pageSize > -1)
            {
                model = model.Skip(startRec).Take(pageSize).ToList();
            }
            else
            {
                model = model.Skip(startRec).ToList();
            }

            return new JsonResult(new { draw = draw, iTotalRecords = totalRecords, iTotalDisplayRecords = totalRecords, data = model });
        }
        public IActionResult CreateChargePoint()
        {
            string id = Request.Form["id"];
            string comment = Request.Form["comment"];
            string stationid = Request.Form["stationid"];
            string name = Request.Form["name"];
            byte[]? Images = null;
            if (string.IsNullOrEmpty(id))
            {
                return Json("รหัส RFID เป็นค่าว่าง กรุณาสแกนบัตร");
            }

            if (Request.Form.Files.Count > 0)
            {
                IFormFileCollection files = Request.Form.Files;
                IFormFile file = files[0];
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    using (var img = Image.FromStream(ms))
                    {
                        // TODO: ResizeImage(img, 100, 100);
                        var newimage = ImageHelper.ResizeImage(img, img.Width / 2, img.Height / 2);
                        Images = ImageHelper.CopyImageToByteArray(newimage);
                    }
                }
            }

            ChargePointModel.CreateChargePoint(new Entities.TblCharger()
            {
                FCode = id,
                FShortName = id,
                FComment = comment,
                FName = name,
                FStationId = Guid.Parse(stationid),
                FImage = Images
            });

            return Json("success");
        }
        public IActionResult UpadateChargePoint()
        {
            string id = Request.Form["id"];
            string name = Request.Form["name"];
            string stationid = Request.Form["stationid"];
            string comment = Request.Form["comment"];

            byte[]? Images = null;

            if (Request.Form.Files.Count > 0)
            {
                IFormFileCollection files = Request.Form.Files;
                for (int i = 0; i < files.Count; i++)
                {
                    IFormFile file = files[i];
                    using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        using (var img = Image.FromStream(ms))
                        {
                            // TODO: ResizeImage(img, 100, 100);
                            var newimage = ImageHelper.ResizeImage(img, img.Width / 2, img.Height / 2);
                            Images = ImageHelper.CopyImageToByteArray(newimage);
                        }
                    }
                }
            }

            var model = ChargePointModel.GetChargePoint(Guid.Parse(id));
            model.FName = name;
            model.FStationId = Guid.Parse(stationid);
            model.FComment = comment;
            if (Images != null)
                model.FImage = Images;
            ChargePointModel.UpdateChargePoint(model);

            return Json("success");
        }
        public IActionResult GetStationSelect()
        {
            var data = StationInfoModel.GetStationInfo();
            data = data.OrderBy(x => x.FName).ToList();
            return new JsonResult(new { data = data });
        }
        
        public IActionResult DeleteChargePoint(string id)
        {
            var model = ChargePointModel.GetChargePoint(Guid.Parse(id));
            ChargePointModel.DeleteChargePoint(model);
            return Json("success");
        }
    }
}

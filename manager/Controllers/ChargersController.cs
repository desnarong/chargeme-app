using manager.Entities;
using manager.Helpers;
using manager.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;

namespace manager.Controllers
{
    public class ChargersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult GetChargerTable()
        {
            var userInfo = JsonConvert.DeserializeObject<TblUser>(HttpContext.Session.GetString("UserData"));

            string draw = Request.Form["draw"][0];
            string order = Request.Form["order[0][column]"].Count > 0 ? Request.Form["order[0][column]"][0] : "0";
            string orderDir = Request.Form["order[0][dir]"].Count > 0 ? Request.Form["order[0][dir]"][0] : "asc";
            int startRec = Convert.ToInt32(Request.Form["start"][0]);
            int pageSize = Convert.ToInt32(Request.Form["length"][0]);

            var model = ChargerModel.GetChargerDatas();
            if (userInfo.FCompanyId != Guid.Empty) model = model.Where(x => x.CompanyId == userInfo.FCompanyId).ToList();
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
        public IActionResult CreateCharger()
        {
            string id = Request.Form["id"];
            string comment = Request.Form["comment"];
            string stationid = Request.Form["stationid"];
            string name = Request.Form["name"];
            byte[]? Images = null;
            if (string.IsNullOrEmpty(id))
            {
                return Json("รหัสเป็นค่าว่างระบุ");
            }

            if (Request.Form.Files.Count > 0)
            {
                IFormFileCollection files = Request.Form.Files;
                IFormFile file = files[0];
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    ms.Position = 0; // 🔑 สำคัญมาก!
                    Images = ImageSharp.ResizeImageWithImageSharp(ms);
                }
            }

            ChargerModel.CreateCharger(new Entities.TblCharger()
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
        public IActionResult UpadateCharger()
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
                    using var inputStream = file.OpenReadStream();

                    // ใช้เมธอดที่ปรับปรุงแล้ว
                    Images = ImageSharp.ResizeImageWithImageSharp(inputStream);
                }
            }

            var model = ChargerModel.GetCharger(Guid.Parse(id));
            model.FName = name;
            model.FStationId = Guid.Parse(stationid);
            model.FComment = comment;
            if (Images != null)
                model.FImage = Images;
            ChargerModel.UpdateCharger(model);

            return Json("success");
        }
        public IActionResult GetStationSelect()
        {
            var data = StationInfoModel.GetStationInfo();
            data = data.OrderBy(x => x.FName).ToList();
            return new JsonResult(new { data = data });
        }

        public IActionResult DeleteCharger(string id)
        {
            var model = ChargerModel.GetCharger(Guid.Parse(id));
            ChargerModel.DeleteCharger(model);
            return Json("success");
        }
    }
}

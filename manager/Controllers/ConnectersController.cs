using manager.Models;
using Microsoft.AspNetCore.Mvc;

namespace manager.Controllers
{
    public class ConnectersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult GetConectorTable()
        {
            string draw = Request.Form["draw"][0];
            string order = Request.Form["order[0][column]"].Count > 0 ? Request.Form["order[0][column]"][0] : "0";
            string orderDir = Request.Form["order[0][dir]"].Count > 0 ? Request.Form["order[0][dir]"][0] : "asc";
            int startRec = Convert.ToInt32(Request.Form["start"][0]);
            int pageSize = Convert.ToInt32(Request.Form["length"][0]);

            var model = ChargerModel.GetConnectorStatusDatas();
            switch (order)
            {
                case "0":
                    model = orderDir == "asc" ? model.OrderBy(x => x.ChargerId).ToList() : model.OrderByDescending(x => x.ChargerId).ToList();
                    break;
                case "1":
                    model = orderDir == "asc" ? model.OrderBy(x => x.ConnectorId).ToList() : model.OrderByDescending(x => x.ConnectorId).ToList();
                    break;
                case "2":
                    model = orderDir == "asc" ? model.OrderBy(x => x.ConnectorName).ToList() : model.OrderByDescending(x => x.ConnectorName).ToList();
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
        public IActionResult UpadateConector()
        {
            string id = Request.Form["id"];
            string name = Request.Form["name"];
            string chargepointid = Request.Form["chargeid"];
            string code = Request.Form["code"];

            var model = ChargerModel.GetConnectorStatus(Guid.Parse(chargepointid), Guid.Parse(id));
            model.FName = name;
            model.FCode = code;
            ChargerModel.UpdateConnectorStatus(model);

            return Json("success");
        }
    }
}

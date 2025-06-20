using manager.Helpers;
using manager.Models;
using Microsoft.AspNetCore.Mvc;

namespace manager.Controllers
{
    public class CompanyController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Holiday(string id)
        {
            Entities.TblCompany companyInfoData = CompanyInfoModel.GetCompanyInfo(Guid.Parse(id));
            return View(companyInfoData);
        }
        public IActionResult GetCompanyTable()
        {
            string draw = Request.Form["draw"][0];
            string order = Request.Form["order[0][column]"].Count > 0 ? Request.Form["order[0][column]"][0] : "0";
            string orderDir = Request.Form["order[0][dir]"].Count > 0 ? Request.Form["order[0][dir]"][0] : "asc";
            int startRec = Convert.ToInt32(Request.Form["start"][0]);
            int pageSize = Convert.ToInt32(Request.Form["length"][0]);
            int totalRecords = 0;


            var model = CompanyInfoModel.GetCompanyInfoes();
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

            return new JsonResult(new { draw = draw, iTotalRecords = totalRecords, iTotalDisplayRecords = totalRecords, data = model });
        }
        public IActionResult CreateStation()
        {
            byte[]? Logo = null;
            string code = Request.Form["code"];
            string name = Request.Form["name"];
            string address = Request.Form["address"];
            string city = Request.Form["city"];
            string country = Request.Form["country"];
            string postcode = Request.Form["postcode"];
            string tel = Request.Form["tel"];

            var company = new Entities.TblCompany();
            company.FName = name;
            company.FAddress = address;
            company.FCity = city;
            company.FProvince = country;
            company.FPostcode = postcode;
            company.FOffice = tel;
            company.FCode = code;

            if (Request.Form.Files.Count > 0)
            {
                IFormFileCollection files = Request.Form.Files;

                var logoFile = files["logo"];   // ดึงไฟล์จาก Key "logo"
                if (logoFile != null)
                {
                    using var inputStream = logoFile.OpenReadStream();
                    Logo = ImageSharp.ResizeImageWithImageSharp(inputStream);
                }
            }

            if (Logo != null)
                company.FLogo = Logo;

            CompanyInfoModel.Create(company);

            return Json("success");
        }
        public IActionResult UpadateStation()
        {
            byte[]? Logo = null;
            string id = Request.Form["id"];
            string name = Request.Form["name"];
            string address = Request.Form["address"];
            string city = Request.Form["city"];
            string country = Request.Form["country"];
            string postcode = Request.Form["postcode"];
            string tel = Request.Form["tel"];

            var company = CompanyInfoModel.GetCompanyInfo(Guid.Parse(id));
            company.FName = name;
            company.FAddress = address;
            company.FCity = city;
            company.FProvince = country;
            company.FPostcode = postcode;
            company.FOffice = tel;

            if (Request.Form.Files.Count > 0)
            {
                IFormFileCollection files = Request.Form.Files;

                var logoFile = files["logo"];   // ดึงไฟล์จาก Key "logo"

                if (logoFile != null)
                {
                    using var inputStream = logoFile.OpenReadStream();
                    Logo = ImageSharp.ResizeImageWithImageSharp(inputStream);
                }
            }
            if (Logo != null)
                company.FLogo = Logo;

            CompanyInfoModel.Update(company);

            return Json("success");
        }
        public IActionResult DeleteStation(string id)
        {
            var company = CompanyInfoModel.GetCompanyInfo(Guid.Parse(id));
            CompanyInfoModel.Delete(company);

            return Json("success");
        }
    }
}

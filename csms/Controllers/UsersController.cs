using csms.Helpers;
using csms.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.Design;
using System.Security.Cryptography;
using System.Text;

namespace csms.Controllers
{
    public class UsersController : Controller
    {
        private readonly ILogger<UsersController> _logger;

        public UsersController(ILogger<UsersController> logger)
        {
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult GetUserTable()
        {
            string draw = Request.Form["draw"][0];
            string order = Request.Form["order[0][column]"][0];
            string orderDir = Request.Form["order[0][dir]"][0];
            int startRec = Convert.ToInt32(Request.Form["start"][0]);
            int pageSize = Convert.ToInt32(Request.Form["length"][0]);
            int totalRecords = 0;

            var model = UserInfoModel.GetUserDatas();
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

            foreach (var item in model)
            {
                var cominfo = CompanyInfoModel.GetCompanyInfo(item.CompanyId ?? Guid.Empty);
                if (cominfo == null)
                    item.CompanyName = "";
                else
                    item.CompanyName = cominfo.FName;
            }
            return new JsonResult(new { draw = draw, iTotalRecords = totalRecords, iTotalDisplayRecords = totalRecords, data = model });
        }
        public IActionResult CreateUser()
        {
            string name = Request.Form["name"];
            string lastname = Request.Form["lastname"];
            string tel = Request.Form["tel"];
            string password = Request.Form["password"];
            string username = Request.Form["username"];
            string companyid = Request.Form["companyid"];

            var user = new Entities.TblUser();
            user.FName = name;
            user.FLastname = lastname;
            user.FMobile = tel;
            user.FPassword = HashPassword(password);
            user.FUsername = username;
            user.FCompanyId = Guid.Parse(companyid);
            user.FEmail = username;
            UserInfoModel.Create(user);

            return Json("success");
        }
        public IActionResult UpadateUser()
        {
            string id = Request.Form["id"];
            string name = Request.Form["name"];
            string lastname = Request.Form["lastname"];
            string mobile = Request.Form["mobile"];
            string username = Request.Form["username"];
            string companyid = Request.Form["companyid"];

            if (string.IsNullOrEmpty(companyid) || companyid == "null") companyid = Guid.Empty.ToString();

            var user = UserInfoModel.GetStationInfo(Guid.Parse(id));
            user.FName = name;
            user.FLastname = lastname;
            user.FMobile = mobile;
            user.FUsername = username;
            user.FEmail = username;
            user.FCompanyId = Guid.Parse(companyid);
            UserInfoModel.Update(user);

            return Json("success");
        }
        public IActionResult ResetPassword()
        {
            string id = Request.Form["id"];
            var user = UserInfoModel.GetStationInfo(Guid.Parse(id));
            user.FPassword = HashPassword("password");
            UserInfoModel.Update(user);

            return Json("success");
        }
        public IActionResult ChangePassword()
        {
            string id = Request.Form["id"];
            string oldpassword = Request.Form["oldpassword"];
            string newpassword = Request.Form["newpassword"];
            var user = UserInfoModel.GetStationInfo(Guid.Parse(id));
            if(user.FPassword == HashPassword(oldpassword))
            {
                user.FPassword = HashPassword(newpassword);
                UserInfoModel.Update(user);
                return Json("success");
            }
            return Json("error");
        }
        public IActionResult DeleteUser(string id)
        {
            var user = UserInfoModel.GetStationInfo(Guid.Parse(id));

            if(user.FUserGroupId == Guid.Parse("63844e55-79bc-4f72-a7cb-c32b826d5134")) return Json("error");

            user.FStatus = 'N';
            UserInfoModel.Update(user);

            return Json("success");
        }
        private string HashPassword(string password)
        {
            // ฟังก์ชันสำหรับ hash รหัสผ่าน
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(passwordBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }
        public IActionResult GetCompanySelect()
        {
            var data = CompanyInfoModel.GetCompanyInfo();
            data = data.OrderBy(x => x.FName).ToList();
            return new JsonResult(new { data = data });
        }
    }
}

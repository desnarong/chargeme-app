using manager.Entities;
using manager.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace manager.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Profile()
        {
            return View();
        }
        public IActionResult GetUserTable()
        {
            var user = HttpContext.Session.GetString(Constants.SessionUserData);
            var userInfo = JsonConvert.DeserializeObject<TblUser>(user);

            string draw = Request.Form["draw"][0];
            string order = Request.Form["order[0][column]"].Count > 0 ? Request.Form["order[0][column]"][0] : "0";
            string orderDir = Request.Form["order[0][dir]"].Count > 0 ? Request.Form["order[0][dir]"][0] : "asc";
            int startRec = Convert.ToInt32(Request.Form["start"][0]);
            int pageSize = Convert.ToInt32(Request.Form["length"][0]);
            int totalRecords = 0;

            var model = UserInfoModel.GetUserDatas();
            if (userInfo.FCompanyId != Guid.Empty) model = model.Where(x => x.UserGroupId == userInfo.FUserGroupId).ToList();

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
        public IActionResult ChangeMyPassword()
        {
            var userID = User?.FindFirstValue(ClaimTypes.NameIdentifier); // หรือ "sub"
            string oldpassword = Request.Form["oldpassword"];
            string newpassword = Request.Form["newpassword"];
            var user = UserInfoModel.GetUser(Guid.Parse(userID));
            if (user.FPassword == HashPassword(oldpassword))
            {
                user.FPassword = HashPassword(newpassword);
                UserInfoModel.Update(user);
                return Json("success");
            }
            return Json("error");
        }
        public IActionResult UpadateMyUser()
        {
            var userID = User?.FindFirstValue(ClaimTypes.NameIdentifier); // หรือ "sub"
            string name = Request.Form["firstnameInput"];
            string lastname = Request.Form["lastnameInput"];
            string mobile = Request.Form["phonenumberInput"];
            string email = Request.Form["emailInput"];
            string address = Request.Form["addressInput"];
            string city = Request.Form["cityInput"];
            string country = Request.Form["countryInput"];
            string zipcode = Request.Form["zipcodeInput"];

            var user = UserInfoModel.GetUser(Guid.Parse(userID));
            user.FName = name;
            user.FLastname = lastname;
            user.FMobile = mobile;
            user.FCity = city;
            user.FEmail = email;
            user.FAddress = address;
            user.FProvince = country;
            user.FPostcode = zipcode;
            UserInfoModel.Update(user);
            HttpContext.Session.SetString(Constants.SessionUserData, JsonConvert.SerializeObject(user));
            return Json("success");
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

            var user = UserInfoModel.GetUser(Guid.Parse(id));
            user.FName = name;
            user.FLastname = lastname;
            user.FMobile = mobile;
            user.FUsername = username;
            user.FEmail = username;
            user.FCompanyId = Guid.Parse(companyid);
            UserInfoModel.Update(user);
            return Json("success");
        }
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            // ดึง userID จาก Claims
            var userID = User?.FindFirstValue(ClaimTypes.NameIdentifier); // หรือ "sub"
            if (string.IsNullOrEmpty(userID))
            {
                return Unauthorized("Invalid token.");
            }

            // ตรวจสอบไฟล์
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
            {
                return BadRequest("Only JPG, JPEG, and PNG files are allowed.");
            }

            try
            {
                // สร้าง path
                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "images");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // ตั้งชื่อไฟล์
                var fileName = $"{userID}_{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadFolder, fileName);

                // Resize ด้วย ImageSharp
                using (var image = Image.Load(file.OpenReadStream()))
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(300, 300),
                        Mode = ResizeMode.Crop
                    }));

                    await image.SaveAsync(filePath, new JpegEncoder { Quality = 80 });
                }

                // URL สำหรับเข้าถึง
                var fileUrl = $"/assets/images/{fileName}";

                // บันทึกลง database
                var user = UserInfoModel.GetUser(Guid.Parse(userID));
                user.FImage = fileUrl;
                UserInfoModel.Update(user);
                HttpContext.Session.SetString(Constants.SessionUserData, JsonConvert.SerializeObject(user));
                return Ok(new
                {
                    Message = "Upload successful.",
                    NewProfilePictureUrl = fileUrl
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        public IActionResult ResetPassword()
        {
            string id = Request.Form["id"];
            var user = UserInfoModel.GetUser(Guid.Parse(id));
            user.FPassword = HashPassword("password");
            UserInfoModel.Update(user);

            return Json("success");
        }
        public IActionResult ChangePassword()
        {
            string id = Request.Form["id"];
            string oldpassword = Request.Form["oldpassword"];
            string newpassword = Request.Form["newpassword"];
            var user = UserInfoModel.GetUser(Guid.Parse(id));
            if (user.FPassword == HashPassword(oldpassword))
            {
                user.FPassword = HashPassword(newpassword);
                UserInfoModel.Update(user);
                return Json("success");
            }
            return Json("error");
        }
        public IActionResult DeleteUser(string id)
        {
            var user = UserInfoModel.GetUser(Guid.Parse(id));

            if (user.FUserGroupId == Guid.Parse("63844e55-79bc-4f72-a7cb-c32b826d5134")) return Json("error");

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
        public IActionResult GetGroupSelect()
        {
            var user = HttpContext.Session.GetString(Constants.SessionUserData);
            var userInfo = JsonConvert.DeserializeObject<TblUser>(user);
            var data = UserGroupInfoModel.GetGroups();
            if (userInfo.FUserGroupId != Guid.Empty)
            {
                data = data.Where(x => x.FId != Guid.Empty).ToList();
            }
           
            data = data.OrderBy(x => x.FName).ToList();
            return new JsonResult(new { data = data });
        }
    }
}

using manager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using manager.Entities;

namespace manager.Controllers
{
    public class AuthController : BaseController
    {
        private readonly HttpClient _client;
        public AuthController(IHttpClientFactory clientFactory, UserManager userManager, ILoggerFactory loggerFactory, IConfiguration config) : base(userManager, loggerFactory, config)
        {
            Logger = loggerFactory.CreateLogger<AuthController>();
            _client = clientFactory.CreateClient("ApiClient");
        }

        // GET: /Account/Login
        //[HttpGet("Login")]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            // ป้องกันไม่ให้วนลูปไปที่ /Auth/Login
            if (!string.IsNullOrEmpty(returnUrl) && returnUrl != "/Auth/Login")
            {
                ViewData["ReturnUrl"] = returnUrl;
            }
            else
            {
                ViewData["ReturnUrl"] = "/Home/Index";
            }
            return View();
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
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(UserModel userModel, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var response = await _client.PostAsJsonAsync("/auth/login", new { Email = userModel.Username, Password = userModel.Password });

                if (!response.IsSuccessStatusCode)
                {
                    return View(userModel);
                }
                var result = await response.Content.ReadAsStringAsync();
                var userInfo = JsonConvert.DeserializeObject<TblUser>(result);
                userModel.UserId = userInfo.FId;
                userInfo.FImage = $"/assets{userInfo.FImage}";
                //get-token
                var session = await _client.GetAsync($"/session/get-token/{userInfo.FId}");

                if (!session.IsSuccessStatusCode)
                {
                    return View(userModel);
                }
                var tokenInfo = await session.Content.ReadAsStringAsync();
                var token = JsonConvert.DeserializeObject<tokenInfo>(tokenInfo);
                userInfo.FToken = $"{token.token}";
                HttpContext.Session.SetString(Constants.SessionAuthToken, userInfo.FToken);
                HttpContext.Session.SetString(Constants.SessionUserData, JsonConvert.SerializeObject(userInfo));
                //return RedirectToLocal(returnUrl);
                //HttpContext.Session.SetString("UserRoleData", rolejson);

                userModel.IsAdmin = true;

                await UserManager.SignIn(this.HttpContext, userModel, false);
                if (userModel != null && !string.IsNullOrWhiteSpace(userModel.Username))
                {
                    Logger.LogInformation("User '{0}' logged in", userModel.Username);
                    //return RedirectToLocal(returnUrl);
                    return Json(new { success = true, company = userInfo.FCompanyId });
                }
                else
                {
                    Logger.LogInformation("Invalid login attempt: User '{0}'", userModel.Username);
                    ModelState.AddModelError(string.Empty, "Invalid login attempt");
                    return View(userModel);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(userModel);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Logout(UserModel userModel)
        {
            Logger.LogInformation("Signing our user '{0}'", userModel.Username);
            await UserManager.SignOut(this.HttpContext);

            return RedirectToAction(nameof(AuthController.Login), "Auth");
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(DashboardsController.Index), Constants.DashboardsController);
            }
        }
    }
    public class tokenInfo { public string? token { get; set; } }
}

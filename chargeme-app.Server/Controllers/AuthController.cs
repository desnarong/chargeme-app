using chargeme_app.Server.DataContext;
using chargeme_app.Server.Entities;
using chargeme_app.Server.Models;
using chargeme_app.Server.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace chargeme_app.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly NpgsqlDbContext _context;
        private readonly TokenService _tokenService;
        private readonly LogService _logService;
        public readonly UserService _userService;
        public AuthController(NpgsqlDbContext context, TokenService tokenService, LogService logService, UserService userService)
        {
            _context = context;
            _tokenService = tokenService;
            _logService = logService;
            _userService = userService;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // ตรวจสอบข้อมูลการเข้าสู่ระบบ
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Email or password cannot be empty.");
            }

            var user = _context.TblUsers.Where(x => x.FEmail == request.Email && x.FPassword == HashPassword(request.Password)).FirstOrDefault();// || x.FUsername == request.Email
            if (user == null)
            {
                return Unauthorized("Invalid credentials.");
            }

            var token = _tokenService.CreateToken(user.FId.ToString(), user.FEmail);
            user.FToken = token;
            _context.TblUsers.Update(user);
            await _context.SaveChangesAsync();

            // บันทึก log
            _logService.LogEvent(user.FId, "ChargingStarted", $"User:{user.FId} started session");
            user.FToken = "";
            if (string.IsNullOrEmpty(user.FImage)) user.FImage = "userimage.png";
            else user.FImage = user.FImage.Replace("assets/", "") ?? "userimage.png";
            user.FPassword = "";
            return new JsonResult(user);
        }
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> logout()
        {
            var userID = User?.FindFirstValue(JwtRegisteredClaimNames.Sid);
            var user = _context.TblUsers.Where(x => x.FId == Guid.Parse(userID)).FirstOrDefault();
            if (user == null)
            {
                return Unauthorized("Invalid credentials.");
            }

            user.FToken = "";
            _context.TblUsers.Update(user);
            await _context.SaveChangesAsync();

            // บันทึก log
            _logService.LogEvent(user.FId, "Auth", $"User:{user.FId} logout");

            return new JsonResult("success");
        }
        [Authorize]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
        {
            if (ModelState.IsValid)
            {
                var userID = User?.FindFirstValue(JwtRegisteredClaimNames.Sid);
                // ตรวจสอบว่า fid เป็น Guid หรือไม่
                if (!Guid.TryParse(userID, out Guid parsedFid))
                {
                    return BadRequest("Fid ไม่ถูกต้อง");
                }
                var existingUser = await _userService.CheckExistingUser(parsedFid);
                if (existingUser != null)
                {
                    // ตรวจสอบว่า email ตรงกับข้อมูลที่อยู่ใน database หรือไม่
                    if (existingUser.FUserGroupId != Guid.Parse("365e6dd9-bfa1-4151-8c7f-42d9139ab73b"))
                    {
                        // ตรวจสอบว่า email ตรงกับข้อมูลที่อยู่ใน database หรือไม่
                        if (existingUser.FEmail == request.Email)
                        {
                            return BadRequest("อีเมลนี้ลงทะเบียนแล้ว");
                        }

                        // ตรวจสอบว่า email นี้มีอยู่ในระบบหรือยัง
                        var isEmailRegistered = _userService.CheckIfEmailExists(request.Email);
                        if (isEmailRegistered)
                        {
                            return Conflict("อีเมลนี้ถูกใช้งานแล้ว");
                        }

                        existingUser = await _userService.NewUser(request);
                    }
                    else
                    {
                        existingUser.FEmail = request.Email;
                        existingUser.FPassword = HashPassword(request.Password);
                        existingUser.FUserGroupId = Guid.Parse("f63caefd-2a43-49da-a6cd-6aa40ce90dd4");
                        existingUser.FUpdated = DateTime.UtcNow;
                        await _userService.UpdateUser(existingUser);
                    }
                }

                return new JsonResult(existingUser);
            }
            return BadRequest("ข้อมูลไม่ถูกต้อง");
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
    }
}

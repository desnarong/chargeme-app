using chargeme_app.Server.DataContext;
using chargeme_app.Server.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace chargeme_app.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        public readonly UserService _userService;
        public UserController(NpgsqlDbContext context, UserService userService)
        {
            _userService = userService;
        }
        [HttpPost("upload-profile-picture")]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            // ตรวจสอบ token และดึง userID
            var userID = User?.FindFirstValue(JwtRegisteredClaimNames.Sid);
            if (string.IsNullOrEmpty(userID))
            {
                return Unauthorized("Invalid token.");
            }

            // ตรวจสอบไฟล์
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            try
            {
                // กำหนดเส้นทางการบันทึกไฟล์ (สามารถเปลี่ยน path ตามต้องการ)
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/users");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // สร้างชื่อไฟล์ใหม่เพื่อป้องกันการชนกัน
                var fileName = $"{userID}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadPath, fileName);

                // อ่านไฟล์ต้นฉบับและทำการ resize
                using (var image = Image.Load(file.OpenReadStream()))
                {
                    // Resize รูปภาพเป็น 300x300 px (ปรับขนาดได้ตามต้องการ)
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(300, 300),
                        Mode = ResizeMode.Crop // หรือใช้ ResizeMode.Max เพื่อคงสัดส่วน
                    }));

                    // บันทึกเป็น JPEG คุณภาพ 80%
                    await image.SaveAsync(filePath, new JpegEncoder { Quality = 80 });
                }

                // URL สำหรับเข้าถึงไฟล์
                var fileUrl = $"/users/{fileName}";

                // สามารถบันทึก URL นี้ลงฐานข้อมูลเพื่อเก็บประวัติรูปโปรไฟล์ได้

                var user = await _userService.GetUser(Guid.Parse(userID));
                user.FImage = fileUrl;
                _ = await _userService.UpdateUser(user);
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
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userID = User?.FindFirstValue(JwtRegisteredClaimNames.Sid);
            if (userID == null)
                return Unauthorized("Invalid token.");

            var user = await _userService.GetUser(Guid.Parse(userID));
            user.FName = request.fName;
            user.FLastname = request.fLastname;
            user.FLanguage = request.language;
            _ = await _userService.UpdateUser(user);

            return Ok(new { message = "Profile updated successfully." });
        }
    }
    public class UpdateProfileRequest
    {
        public string? fName { get; set; }
        public string? fLastname { get; set; }
        public string? language { get; set; }
    }

}

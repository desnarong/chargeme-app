using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace chargeme_app.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UserSessionController : ControllerBase
    {
        [HttpGet("status")]
        public IActionResult GetSessionStatus()
        {
            // ดึง GuestIdentifier จาก token
            var userID = User?.FindFirstValue(JwtRegisteredClaimNames.Sid);

            if (string.IsNullOrEmpty(userID))
            {
                return Unauthorized("Invalid token.");
            }

            // ใช้ guestIdentifier ในการตรวจสอบสถานะ session
            return Ok(new { Status = "Active", GuestIdentifier = userID });
        }
    }
}

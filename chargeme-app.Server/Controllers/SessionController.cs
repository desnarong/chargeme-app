using chargeme_app.Server.DataContext;
using chargeme_app.Server.Entities;
using chargeme_app.Server.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Emit;

namespace chargeme_app.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SessionController : ControllerBase
    {
        private readonly NpgsqlDbContext _context;
        private readonly TokenService _tokenService;
        private readonly LogService _logService;
        public SessionController(NpgsqlDbContext context, TokenService tokenService, LogService logService)
        {
            _context = context;
            _tokenService = tokenService;
            _logService = logService;
        }
       
        [HttpPost("end")]
        public IActionResult EndSession()
        {
            var identifier = HttpContext.Items["identifier"]?.ToString();
            if (string.IsNullOrEmpty(identifier))
            {
                return BadRequest("Identifier is missing.");
            }
            return Ok();
        }

        [HttpPost("guest-session")]
        public async Task<IActionResult> CreateGuestSession()
        {
            // สร้าง GuestSession และบันทึกลงฐานข้อมูล
            var session = new TblUser() { FUserGroupId = Guid.Parse("365e6dd9-bfa1-4151-8c7f-42d9139ab73b") };

            _context.TblUsers.Add(session);
            await _context.SaveChangesAsync();

            var token = _tokenService.CreateToken(session.FId.ToString(), session.FEmail);
            session.FToken = token;
            _context.TblUsers.Update(session);
            await _context.SaveChangesAsync();

            // บันทึก log
            _logService.LogEvent(session.FId, "ChargingStarted", $"Guest user started session");
            return Ok(new { message = session.FId.ToString() });
        }
        [HttpGet("get-token/{id}")]
        public async Task<IActionResult> GetTokenFromCookie(string id)
        {
            var user = _context.TblUsers.FirstOrDefault(x => x.FId == Guid.Parse(id)) ?? new TblUser();

            // Decode the token to get the expiration time
            

            if (string.IsNullOrEmpty(user.FToken))
            {
                var token = _tokenService.CreateToken(user.FId.ToString(), user.FEmail);
                user.FToken = token;
                _context.TblUsers.Update(user);
                _ = await _context.SaveChangesAsync();
            }
           
            if (user.FToken != null)
            {
                var tokendecode = _tokenService.ValidToken(user.FToken);
                if (tokendecode.ValidTo < DateTime.UtcNow)
                {
                    var token = _tokenService.CreateToken(user.FId.ToString(), user.FEmail);
                    user.FToken = token;
                    _context.TblUsers.Update(user);
                    _ = await _context.SaveChangesAsync();
                }
                return Ok(new { token = user.FToken });
            }

            return BadRequest(new { message = "No token found" });
        }
        [HttpGet("renewtoken/{id}")]
        public async Task<IActionResult> GetRenewToken(string id)
        {
            var user = _context.TblUsers.FirstOrDefault(x => x.FId == Guid.Parse(id)) ?? new TblUser();
            var token = _tokenService.CreateToken(user.FId.ToString(), user.FEmail);
            user.FToken = token;
            _context.TblUsers.Update(user);
            _ = await _context.SaveChangesAsync();

            if (user.FToken != null)
            {
                return Ok(new { token = user.FToken });
            }

            return BadRequest(new { message = "No token found" });
        }
    }
}

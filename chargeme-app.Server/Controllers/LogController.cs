using chargeme_app.Server.DataContext;
using chargeme_app.Server.Models;
using chargeme_app.Server.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace chargeme_app.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class LogController : ControllerBase
    {
        private readonly NpgsqlDbContext _context;
        private readonly TokenService _tokenService;
        private readonly LogService _logService;
        public LogController(NpgsqlDbContext context, TokenService tokenService, LogService logService)
        {
            _context = context;
            _tokenService = tokenService;
            _logService = logService;
        }

        [HttpPost("log-event")]
        public IActionResult LogEvent([FromBody] LogEventRequest request)
        {
            var userID = User?.FindFirstValue(JwtRegisteredClaimNames.Sid);
            // เรียกใช้งาน LogService เพื่อบันทึก log
            _logService.LogEvent(Guid.Parse(userID), request.Event, request.Description);

            return Ok();
        }
    }
}

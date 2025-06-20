using chargeme_app.Server.DataContext;
using chargeme_app.Server.Service;
using Microsoft.EntityFrameworkCore;

namespace chargeme_app.Server.Middleware
{
    public class IdentifierMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TokenService _tokenService;
        public IdentifierMiddleware(RequestDelegate next, TokenService tokenService)
        {
            _next = next;
            _tokenService = tokenService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Cookies.TryGetValue("guestToken", out var identifier))
            {
                // ตรวจสอบว่า guestIdentifier มีอยู่ในฐานข้อมูลและยังไม่หมดอายุ
                var isValid = await IsValidIdentifierAsync(identifier);
                if (isValid)
                {
                    context.Items["GuestIdentifier"] = identifier;
                }
                else
                {
                    context.Items["GuestIdentifier"] = null;
                }
            }
            else
            {
                context.Items["GuestIdentifier"] = null;
            }
            await _next(context);
        }

        private async Task<bool> IsValidIdentifierAsync(string identifier)
        {
            //var user = await _context.TblUsers
            //    .FirstOrDefaultAsync(s => s.FId == Guid.Parse(identifier));
            //_tokenService.ValidToken("");
            return true;
        }
    }
}

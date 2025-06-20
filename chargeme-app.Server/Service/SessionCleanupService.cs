using chargeme_app.Server.DataContext;
using Microsoft.EntityFrameworkCore;

namespace chargeme_app.Server.Service
{
    public class SessionCleanupService : BackgroundService
    {
        private readonly NpgsqlDbContext _context;
        private readonly TokenService _tokenService;

        public SessionCleanupService(NpgsqlDbContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CleanUpExpiredSessions();
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // รันทุกๆ 1 ชั่วโมง
            }
        }

        private async Task CleanUpExpiredSessions()
        {
            //using (var scope = _serviceProvider.CreateScope())
            //{
            //    //var context = scope.ServiceProvider.GetRequiredService<NpgsqlDbContext>();
            //    //var expiredSessions = await context.TblUsers
            //    //    .Where(s => DateTime.FromFileTime((s.FUpdated ?? s.FCreated).Ticks).AddDays(7) < DateTime.UtcNow)
            //    //    .ToListAsync();

            //    //if (expiredSessions.Any())
            //    //{
            //    //    context.TblUsers.RemoveRange(expiredSessions);
            //    //    await context.SaveChangesAsync();
            //    //}
            //}
        }
    }
}

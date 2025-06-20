using chargeme_app.Server.DataContext;
using chargeme_app.Server.Entities;

namespace chargeme_app.Server.Service
{
    public class LogService
    {
        private readonly NpgsqlDbContext _context;

        public LogService(NpgsqlDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task LogEvent(Guid userId, string eventType, string description)
        {
            //try
            //{
            //    var log = new TblEventLog
            //    {
            //        FCreated = DateTime.UtcNow,
            //        FDescription = description,
            //        FUserId = userId,
            //        FEventTypeId = Guid.NewGuid()
            //    };

            //    _context.TblEventLogs.Add(log);
            //    await _context.SaveChangesAsync();
            //}
            //catch (Exception ex)
            //{
            //    // Log or handle exception
            //    //Console.WriteLine(ex.Message);
            //    //throw; // Re-throw the exception if needed
            //}
        }
    }
}

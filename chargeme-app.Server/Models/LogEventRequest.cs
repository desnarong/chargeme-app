namespace chargeme_app.Server.Models
{
    public class LogEventRequest
    {
        public required string Event { get; set; }
        public required string Description { get; set; }
    }
}

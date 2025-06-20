using Microsoft.AspNetCore.SignalR;

namespace csms.Hubs
{
    public class ChargePointHub : Hub
    {
        public async Task SendMessage(string message)
        {
            if (Clients != null) { await Clients.All.SendAsync("ChargePointMessage", message); }
        }
    }
}

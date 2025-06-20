using Microsoft.AspNetCore.SignalR;

namespace manager.Hubs
{
    public class ChargerHub : Hub
    {
        public async Task SendMessage(string message)
        {
            if (Clients != null) { await Clients.All.SendAsync("ChargerMessage", message); }
        }
    }
}

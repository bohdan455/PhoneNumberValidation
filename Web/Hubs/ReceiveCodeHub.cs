using Microsoft.AspNetCore.SignalR;

namespace Web.Hubs
{
    public class ReceiveCodeHub : Hub
    {
        public async Task ValidateNumber(string message, string connectionId)
        {
            await Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
            
        }
        public string GetConnectionId() => Context.ConnectionId;
    }
}

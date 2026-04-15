using Microsoft.AspNetCore.SignalR;

namespace Messaging.Hub.Infra;

public class ConnectorHub : Microsoft.AspNetCore.SignalR.Hub
{
    public override Task OnConnectedAsync()
    {
        return base.OnConnectedAsync();
    }

    public async Task SendMessage(string user, string message)
    {
        // Broadcast the message to all connected clients
        await Clients.All.SendAsync("command-message", message);
    }
}
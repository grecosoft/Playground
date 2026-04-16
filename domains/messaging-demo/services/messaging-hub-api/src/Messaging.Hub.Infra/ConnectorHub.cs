using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Messaging.Hub.Infra;

public class ConnectorHub(ILogger<ConnectorHub> logger) : Microsoft.AspNetCore.SignalR.Hub
{
    public override Task OnConnectedAsync()
    {
        logger.LogInformation(
            "Connected - UserIdentifier: {UserIdentifier}",
            Context.UserIdentifier);
        
        return base.OnConnectedAsync();
    }

    public async Task SendMessage(string user, string message)
    {
        // Broadcast the message to all connected clients
        await Clients.All.SendAsync("command-message", message);
    }
}
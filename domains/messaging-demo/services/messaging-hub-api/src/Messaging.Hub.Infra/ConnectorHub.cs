using Messaging.Hub.Domain;
using Microsoft.Extensions.Logging;

namespace Messaging.Hub.Infra;

public class ConnectorHub(
    ILogger<ConnectorHub> logger,
    IConnectionManager connectionManager) : Microsoft.AspNetCore.SignalR.Hub
{
    public override Task OnConnectedAsync()
    {
        logger.LogDebug(
            "Agent: {Identifier} Connected as: {ConnectionId}",
            Context.UserIdentifier, 
            Context.ConnectionId);
        
        connectionManager.AddConnection(Context.UserIdentifier!, Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        connectionManager.RemoveConnection(Context.UserIdentifier!);

        if (exception == null)
        {
            logger.LogDebug(
                "Agent: {Identifier} with: {ConnectionId} disconnected",
                Context.UserIdentifier, 
                Context.ConnectionId);
            
            return base.OnDisconnectedAsync(exception);
        }
        
        logger.LogError(exception,
            "Agent: {Identifier} with: {ConnectionId} disconnected",
            Context.UserIdentifier, 
            Context.ConnectionId);
         
        return base.OnDisconnectedAsync(exception);
    }

    public string SendResponseToCommand(string correlationId, string response)
    {
        return Guid.NewGuid().ToString();
    }
}

 
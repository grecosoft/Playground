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
}

// Add methods here to send both an RPC style method that returns a response to the caller,
// and a fire-and-forget method that sends a message to the client without expecting a response.

// Add method that can be called from the client passing a repose and the corresponding CorrelationId
// Then look up the correlationId in the pending request repository and send replay back to clling service. 
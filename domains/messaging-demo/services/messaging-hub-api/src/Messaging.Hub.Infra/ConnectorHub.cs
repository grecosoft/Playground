using Common.Messaging;
using Common.Messaging.Commands;
using Messaging.Hub.Domain;
using Microsoft.Extensions.Logging;

namespace Messaging.Hub.Infra;

public class ConnectorHub(
    ILogger<ConnectorHub> logger,
    IConnectionManager connectionManager,
    ICommandRepository commandRepository,
    ICommandMessaging commandMessaging) : Microsoft.AspNetCore.SignalR.Hub
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

    /// <summary>
    /// Method invoked by the clients to send response back to the command issued by the API.
    /// The correlationId is used to correlate the response with the command. 
    /// </summary>
    /// <param name="correlationId"></param>
    /// <param name="response"></param>
    /// <returns></returns>
    public async Task SendResponseToCommand(string correlationId, string response)
    {
        var context = await commandRepository.LoadContextContext(correlationId, Context.ConnectionAborted);
        context.SetResponse(BinaryData.FromString(response));
        
        await commandMessaging.SendResponseToCommandAsync(context, Context.ConnectionAborted);
    }
}

 
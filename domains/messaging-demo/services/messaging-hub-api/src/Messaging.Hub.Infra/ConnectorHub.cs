using Common.Messaging;
using Common.Messaging.Commands;
using Messaging.Hub.Domain;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Messaging.Hub.Infra;

public class ConnectorHub(
    IOptions<MessagingConfig> messagingOptions,
    ILogger<ConnectorHub> logger,
    ICommandValidationService validationService,
    IConnectionManager connectionManager,
    ICommandRepository commandRepository,
    ICommandMessaging commandMessaging) : Microsoft.AspNetCore.SignalR.Hub
{
        
    private readonly MessagingConfig _msgConfig = messagingOptions.Value;
    
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
        var context = await commandRepository.LoadCommandContext(correlationId, Context.ConnectionAborted);
        
        logger.LogDebug(
            "Received Response: [{Destination}<={Source}]({Namespace}:{CorrelationId})", 
            _msgConfig.ServiceName,
            Context.UserIdentifier,
            context.CommandNamespace,
            correlationId);
        
        var valResults = validationService.ValidateResponse(context.CommandNamespace, response);
        if (valResults.IsValid)
        {
            context.SetResponseData(BinaryData.FromString(response));
            await commandMessaging.SendResponseToCommandAsync(context, Context.ConnectionAborted);
            return;
        }
 
        logger.LogDebug(
            "Validation failed for response: [{Destination}<={Source}]({Namespace}:{CorrelationId}",
            _msgConfig.ServiceName,
            Context.UserIdentifier,
            context.CommandNamespace,
            correlationId);
        
        // Notify the external service that the submitted response failed validation:
        await Clients.Caller.SendAsync("command.error", "Invalid Response");
    }
}

 
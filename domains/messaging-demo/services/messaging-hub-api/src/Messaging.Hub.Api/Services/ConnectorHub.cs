using Common.Messaging;
using Common.Messaging.Commands;
using Messaging.Hub.Api.Models;
using Messaging.Hub.Domain;
using Microsoft.Extensions.Options;

namespace Messaging.Hub.Api.Services;

/// <summary>
/// SignalR Hub defining methods that can be invoked from external Cloud Connectors providing
/// integration with internal platform services.
///
/// - Used to send commands received from internal services to external Cloud Connectors.
/// - Allows external Cloud connector to respond to a previously received command, which is
///   then routed back to the originating service.
/// 
/// </summary>
/// <param name="messagingOptions">Configuration options.</param>
/// <param name="logger">Configured logger.</param>
/// <param name="validationService">Allows validation of commands and response.</param>
/// <param name="connectorHubManager">Service that manages the currently connected connectors.</param>
/// <param name="commandRepository">Repository used to load commands having pending responses.</param>
/// <param name="commandMessaging">Service used to delegate responds back to the originating service.</param>
public class ConnectorHub(
    IOptions<MessagingConfig> messagingOptions,
    ILogger<ConnectorHub> logger,
    ICommandValidationService validationService,
    IConnectorHubManager connectorHubManager,
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
        
        connectorHubManager.AddConnection(Context.UserIdentifier!, Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        connectorHubManager.RemoveConnection(Context.UserIdentifier!, Context.ConnectionId);

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
    /// Method invoked by clients to send response back to the service that initially sent the command.
    /// </summary>
    /// <param name="correlationId">The correlationId is used to correlate the response with the original command.</param>
    /// <param name="response">The response to pending command.</param>
    /// <returns>The result of processing the response.</returns>
    public async Task<CommandReplyResultModel> SendResponseToCommand(string correlationId, string response)
    {
        var context = await commandRepository.LoadCommandContext(correlationId, Context.ConnectionAborted);
        if (context is not null)
        {
            return await SendResponseToOriginatingService(context, response);
        }
        
        logger.LogDebug(
            "Received Response: [{Destination}<={Source}] for unknown correlation id: {CorrelationId}", 
            _msgConfig.ServiceName,
            Context.UserIdentifier,
            correlationId);
            
        // Notify connector client that the submitted response is invalid since the correlation id is unknown:
        return CommandReplyResultModel.Failed(
            correlationId,
            string.Empty,
            "Unknown correlation id");
    }

    private async Task<CommandReplyResultModel> SendResponseToOriginatingService(
        CommandContext context,
        string response)
    {
        logger.LogDebug(
            "Received Response: [{Destination}<={Source}]({Namespace}:{CorrelationId})", 
            _msgConfig.ServiceName,
            Context.UserIdentifier,
            context.CommandNamespace,
            context.CorrelationId);
        
        // If the received response fails validation, send validation issues
        // back to the external service.
        var valResults = validationService.ValidateResponse(context.CommandNamespace, response);
        if (!valResults.IsValid)
        {
            logger.LogDebug(
                "Validation failed for response: [{Destination}<={Source}]({Namespace}:{CorrelationId}",
                _msgConfig.ServiceName,
                Context.UserIdentifier,
                context.CommandNamespace,
                context.CorrelationId);
        
            // Notify the external service that the submitted response failed validation:
            return CommandReplyResultModel.Failed(
                context.CorrelationId,
                context.CommandNamespace,
                "Validation failed",
                new ValidationErrorModel(valResults.Errors));
        }
        
        // Route the response from the external service back to the originating service
        // that issued the command:
        try
        {
            context.SetResponseData(BinaryData.FromString(response));
            await commandMessaging.SendResponseToCommandAsync(context, Context.ConnectionAborted);
            return CommandReplyResultModel.Success(context.CorrelationId, context.CommandNamespace);
        }
        catch
        {
            return CommandReplyResultModel.Failed(
                context.CorrelationId,
                context.CommandNamespace,
                "Unexpected error processing response");
        }
    }
}

 
using System.Collections.Concurrent;
using System.Text.Json;
using Common.Messaging.Commands;
using Common.Messaging.Entities;
using Connector.Hub.Api.Models;
using Connector.Hub.Domain;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace Connector.Hub.Api.Services;

/// <summary>
/// Service that manages the currently connected connectors and sending commands to them.
/// This is used by command handlers to send commands to the connectors.
/// </summary>
/// <param name="logger">The configured logger.</param>
/// <param name="hub">The SignalR Hub managing communication with external connectors.</param>
/// <param name="validationService">Service used to validate commands and responses.</param>
public class ConnectorHubManager(
    ILogger<ConnectorHubManager> logger,
    IOptions<ConnectorHubConfig> connectorHubOptions,
    IHubContext<ConnectorHub> hub,
    ICommandValidationService validationService) : IConnectorHubManager
{
    private const string ReplayValidationErrorMethod = "command.reply.validation.error";
    private readonly ConcurrentDictionary<string, string> _connections = new();
    private readonly ConnectorHubConfig _connectorHubConfig = connectorHubOptions.Value;

    public void AddConnection(string connectorId, string connectionId)
    {
        logger.LogDebug(
            "Connector: {ConnectorId} with connectionId: {ConnectionId} connected.",
            connectorId, connectionId);
        
        _connections.TryAdd(connectorId, connectionId);
    }

    public void RemoveConnection(string connectorId, string connectionId)
    {
        logger.LogDebug(
            "Connector: {ConnectorId} with connectionId: {ConnectionId} disconnected.",
            connectorId, connectionId);
        
        _connections.TryRemove(connectorId, out _);
    }

    public string? GetConnection(string connectorId) => _connections.GetValueOrDefault(connectorId);
    
    public async Task<TResponse?> SendCommandWaitResponseAsync<TResponse>(
        string connectorId, 
        CommandContext context,
        ICommandMessage<TResponse> command,
        CancellationToken ct)
    {
        var connectionId = GetConnection(connectorId);
        LogExternalConnectorCall(connectorId, context, connectionId);
        
        if (connectionId is null)
        {
            // Since this is an RPC style of message and the calling service is waiting for
            // a response, return an error indicating the connector is not connected.
            context.SetResponseError($"No connection found for connector {connectorId}");
            return default;
        }
        
        var clientResponse = await SendCommand(connectionId, context, command, ct);
        
        var valResults = validationService.ValidateResponse(context.CommandNamespace, clientResponse);
        if (valResults.IsValid)
        {
            return JsonSerializer.Deserialize<TResponse>(clientResponse);
        }
        
        // Let service sending command know that the response the connector replied
        // with was not valid. 
        context.SetResponseError(valResults.ErrorMessage);
            
        // Notify connector that the response they sent was invalid.  Since this is an RPC method, there
        // is really nothing the client can do at this point, so sending validation error so they can log.
        var replyResult = CommandReplyResultModel.Failed(
            context.CorrelationId,
            context.CommandNamespace,
            "Validation Failed",
            new ValidationErrorModel(valResults.Errors));
        
        await hub.Clients
            .Client(connectionId)
            .SendAsync(ReplayValidationErrorMethod, context.CorrelationId, replyResult, ct);

        return default;
    }

    public async Task SendCommandFutureResponseAsync<TResponse>(
        string connectorId,
        CommandContext context,
        ICommandMessage<TResponse> command,
        CancellationToken ct)
    {
        var connectionId = GetConnection(connectorId);
        LogExternalConnectorCall(connectorId, context, connectionId);
        
        // Save the command context so a replay to the command from external connector
        // can be sent back to the originating service with the correct correlation ID and namespace.
        await context.CommandRepository.SaveCommandContext(context, ct);
        
        // If the connector is connected send the command.  Otherwise, it will be saved and later sent.
        if (connectionId is null)
        {
            return;
        }
        
        await hub.Clients
            .Client(connectionId)
            .SendAsync(
                context.CommandNamespace, 
                context.CorrelationId,
                command, 
                ct);
    }
    
    private async Task<string> SendCommand<TResponse>(
        string connectionId,
        CommandContext context,
        ICommandMessage<TResponse> command,
        CancellationToken ct)
    {
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(_connectorHubConfig.ConnectorReplyTimeoutSeconds));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);
        
        try
        { 
            return await hub.Clients
                .Client(connectionId)
                .InvokeAsync<string>(context.CommandNamespace, context.CorrelationId, command, linkedCts.Token);
        }
        catch (OperationCanceledException)
        {
            if (timeoutCts.IsCancellationRequested)
            {
                throw new TimeoutException($"Timeout on {connectionId} sending RPC command.");
            }

            throw;
        }
    }

    private void LogExternalConnectorCall(string connectorId, CommandContext context, string? connectionId)
    {
        logger.LogDebug(
            "Sending command to connector: {ConnectorId} with connectionId: {ConnectionId}. " + 
            "Command Namespace: {CommandNamespace}, CorrelationId: {CorrelationId}",
            connectorId, connectionId ?? "Not Connected", context.CommandNamespace, context.CorrelationId);
    }
}
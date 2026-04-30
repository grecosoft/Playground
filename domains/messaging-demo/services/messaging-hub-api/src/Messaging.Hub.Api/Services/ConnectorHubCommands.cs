using System.Text.Json;
using Common.Messaging.Commands;
using Common.Messaging.Entities;
using Messaging.Hub.Api.Models;
using Messaging.Hub.Domain;
using Microsoft.AspNetCore.SignalR;

namespace Messaging.Hub.Api.Services;

/// <summary>
/// Class delegated to from command handlers to send commands to external clients
/// connected to the hub.
/// </summary>
/// <param name="hub">The SignalR hub.</param>
/// <param name="context">The current context of the received command from an internal service.</param>
public class ConnectorHubCommands(
    IHubContext<ConnectorHub> hub,
    CommandContext context)
{
    private const string ReplayValidationErrorMethod = "command.reply.validation.error";
    
    /// <summary>
    /// Sends a command to an external connected client and waits for a response.  If the response is not valid,
    /// the client is notified of the issue.  Since this is an RPC style of command the communication between
    /// the hub and client there is nothing additional the client can do at this point, but notified that the
    /// response they sent was not valid.  The issue is also sent back to the originating service so it can log
    /// or handle as needed.
    /// </summary>
    /// <param name="connectionId">The connection of the external client to send command.</param>
    /// <param name="command">The command to send.</param>
    /// <param name="validationService">Service used to validate command response returned from external client.</param>
    /// <param name="ct">Calculation token.</param>
    /// <typeparam name="TResponse">Response if valid. Otherwise, null.</typeparam>
    /// <returns></returns>
    public async Task<TResponse?> SendCommandWaitResponse<TResponse>(
        string connectionId,
        ICommandMessage<TResponse> command,
        ICommandValidationService validationService,
        CancellationToken ct)
    {
        var clientResponse = await SendCommandWaitResponse(connectionId, command, ct);
        
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

    private async Task<string> SendCommandWaitResponse<TResponse>(
        string connectionId,
        ICommandMessage<TResponse> command,
        CancellationToken ct)
    {
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(4));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);
        
        // TODO: add log with connection info.
        
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

    public Task SendCommandFutureResponse<TResponse>(
        string connectionId,
        ICommandMessage<TResponse> command,
        CancellationToken ct)
    {
        // TODO: add log with connection info.
        
        return hub.Clients
            .Client(connectionId!)
            .SendAsync(
                context.CommandNamespace, 
                context.CorrelationId,
                command, 
                ct);
    }
}
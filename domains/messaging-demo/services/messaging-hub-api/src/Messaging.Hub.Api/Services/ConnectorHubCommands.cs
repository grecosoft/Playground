using System.Text.Json;
using Common.Messaging.Commands;
using Common.Messaging.Entities;
using Messaging.Hub.Domain;
using Microsoft.AspNetCore.SignalR;

namespace Messaging.Hub.Api.Services;

/// <summary>
/// Class delegated to from command handlers to send commands to external clients
/// connected to the hub.
/// </summary>
/// <param name="hub">The SignalR hub.</param>
/// <param name="context">The current context of the received command from an interal service.</param>
public class ConnectorHubCommands(
    IHubContext<ConnectorHub> hub,
    CommandContext context)
{
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
        // TODO:  Create timeout cancellation token and join to passed token...
        
        var clientResponse = await hub.Clients
            .Client(connectionId)
            .InvokeAsync<string>(context.CommandNamespace, context.CorrelationId, command, ct);
        
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
        await hub.Clients
            .Client(connectionId)
            .SendAsync("command.error", context.CorrelationId, "Invalid", ct);

        return default;
    }

    public Task SendCommandFutureResponse<TResponse>(
        string connectionId,
        ICommandMessage<TResponse> command,
        CancellationToken ct)
    {
        return hub.Clients
            .Client(connectionId!)
            .SendAsync(
                context.CommandNamespace, 
                context.CorrelationId,
                command, 
                ct);
    }
}
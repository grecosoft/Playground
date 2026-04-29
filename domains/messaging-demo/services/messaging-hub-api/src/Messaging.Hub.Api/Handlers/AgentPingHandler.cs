using System.Text.Json;
using Common.Messaging.Commands;
using Messaging.Demo.Common.Commands;
using Messaging.Hub.Api.Services;
using Messaging.Hub.Domain;
using Microsoft.AspNetCore.SignalR;

namespace Messaging.Hub.Api.Handlers;

public class AgentPingHandler(
    IHubContext<ConnectorHub> connectorHub,
    IConnectionManager connectionManager,
    ICommandValidationService validationService) : CommandHandlerBase<AgentPingCommand, AgentPingResponse>
{
    protected override async Task HandleMessage(
        AgentPingCommand command,
        CommandContext context,
        CancellationToken cancellationToken)
    {
        var agentConnectionId = connectionManager.GetConnection(command.AgentId);
        
        var agentResponse = await connectorHub.Clients
            .Client(agentConnectionId!)
            .InvokeAsync<string>(context.CommandNamespace, context.CorrelationId, command, cancellationToken);
        
        var valResults = validationService.ValidateResponse(context.CommandNamespace, agentResponse);
        if (!valResults.IsValid)
        {
            // Let service sending command know that the response the connector replied
            // with was not valid. 
            context.SetResponseError(valResults.ErrorMessage);
            
            // Notify connector that the response they sent was invalid.  Since this is an RPC method, there
            // is really nothing the client can do at this point, so sending validation error so they can log.
            await connectorHub.Clients
                .Client(agentConnectionId!)
                .SendAsync("command.error", context.CorrelationId, "Invalid", cancellationToken);
            return;
        }
        
        var response = JsonSerializer.Deserialize<AgentPingResponse>(agentResponse);
        if (response is not null)
        {
            SetResponse(context, response);
        }
    }
}
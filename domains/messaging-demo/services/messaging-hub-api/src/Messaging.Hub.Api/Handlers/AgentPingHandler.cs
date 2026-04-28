using System.Text.Json;
using Common.Messaging.Commands;
using Messaging.Demo.Common.Commands;
using Messaging.Hub.Domain;
using Messaging.Hub.Infra;
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
            context.SetResponseError(valResults.ErrorMessage);
            return;
        }
        
        var response = JsonSerializer.Deserialize<AgentPingResponse>(agentResponse);
        if (response is not null)
        {
            SetResponse(context, response);
        }
    }
}
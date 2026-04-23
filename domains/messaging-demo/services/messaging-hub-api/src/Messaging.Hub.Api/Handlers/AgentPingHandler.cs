using Common.Messaging.Commands;
using Messaging.Demo.Common.Commands;
using Messaging.Hub.Domain;
using Messaging.Hub.Infra;
using Microsoft.AspNetCore.SignalR;

namespace Messaging.Hub.Api.Handlers;

public class AgentPingHandler(
    IHubContext<ConnectorHub> connectorHub,
    IConnectionManager connectionManager) : CommandHandlerBase<AgentPingCommand, AgentPingResponse>
{
    protected override async Task HandleMessage(
        AgentPingCommand command,
        CommandContext context,
        CancellationToken cancellationToken)
    {
        var agentConnectionId = connectionManager.GetConnection(command.AgentId);
        
        var agentResponse = await connectorHub.Clients
            .Client(agentConnectionId!)
            .InvokeAsync<AgentPingResponse>(context.CommandNamespace, context.CorrelationId, command, cancellationToken);
        
        context.SetResponse(agentResponse);
    }
}
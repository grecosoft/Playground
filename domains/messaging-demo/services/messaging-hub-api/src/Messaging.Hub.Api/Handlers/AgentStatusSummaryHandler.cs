using Common.Messaging.Commands;
using Messaging.Demo.Common.Commands;
using Messaging.Hub.Domain;
using Messaging.Hub.Infra;
using Microsoft.AspNetCore.SignalR;

namespace Messaging.Hub.Api.Handlers;

public class AgentStatusSummaryHandler(
    IHubContext<ConnectorHub> connectorHub,
    IConnectionManager connectionManager) : CommandHandlerBase<AgentStatusSummaryCommand, AgentStatusSummaryResponse>
{
    protected override async Task HandleMessage(
        AgentStatusSummaryCommand command,
        CommandContext context,
        CancellationToken cancellationToken)
    {
        var agentConnectionId = connectionManager.GetConnection(command.AgentId);
        
        await context.CommandRepository.SaveCommand(context, cancellationToken);
        
        await connectorHub.Clients
            .Client(agentConnectionId!)
            .SendAsync(
                context.CommandNamespace, 
                context.CorrelationId,
                command, 
                cancellationToken);
    }
}
using Common.Messaging.Commands;
using Messaging.Demo.Common.Commands;
using Messaging.Hub.Api.Services;
using Messaging.Hub.Domain;
using Microsoft.AspNetCore.SignalR;

namespace Messaging.Hub.Api.Handlers;

public class AgentStatusSummaryHandler(
    IHubContext<ConnectorHub> connectorHub,
    IConnectionManager connectionManager) 
    : CommandHandlerBase<AgentStatusCommand, AgentStatusSummaryResponse>
{
    protected override async Task HandleMessage(
        AgentStatusCommand command,
        CommandContext context,
        CancellationToken cancellationToken)
    {
        var agentConnectionId = connectionManager.GetConnection(command.AgentId);
        
        // This is an async type of command where the response will not be known until sometime in the future.
        // The command is saved and a response will be sent to the originating service when the client sends a
        // response back to the hub with a matching correlation ID.
        var hubCommands = new ConnectorHubCommands(connectorHub, context);
        
        await context.CommandRepository.SaveCommandContext(context, cancellationToken);
        await hubCommands.SendCommandFutureResponse(agentConnectionId!, command, cancellationToken);
    }
}
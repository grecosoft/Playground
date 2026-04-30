using Common.Messaging.Commands;
using Connector.Hub.Api.Services;
using Messaging.Demo.Common.Commands;

namespace Connector.Hub.Api.Handlers;

public class AgentStatusSummaryHandler(IConnectorHubManager connectionManager) 
    : CommandHandlerBase<AgentStatusCommand, AgentStatusSummaryResponse>
{
    protected override Task HandleMessage(
        AgentStatusCommand command,
        CommandContext context,
        CancellationToken ct)
    {
        // This is an async type of command where the response will not be known until sometime in the future.
        // The command is saved and a response will be sent to the originating service when the client sends a
        // response back to the hub with a matching correlation ID.
        return connectionManager.SendCommandFutureResponseAsync(command.AgentId, context, command, ct);
    }
}
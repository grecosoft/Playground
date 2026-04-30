using Common.Messaging.Commands;
using Connector.Hub.Api.Services;
using Messaging.Demo.Common.Commands;

namespace Connector.Hub.Api.Handlers;

public class ConnectorStatusHandler(IConnectorHubManager connectionManager) 
    : CommandHandlerBase<ConnectorStatusCommand, ConnectorStatusResponse>
{
    protected override Task HandleMessage(
        ConnectorStatusCommand command,
        CommandContext context,
        CancellationToken ct)
    {
        // This is an async type of command where the response will not be known until sometime in the future.
        // The command is saved and a response will be sent to the originating service when the client sends a
        // response back to the hub with a matching correlation ID.
        return connectionManager.SendCommandFutureResponseAsync(command.ConnectorId, context, command, ct);
    }
}
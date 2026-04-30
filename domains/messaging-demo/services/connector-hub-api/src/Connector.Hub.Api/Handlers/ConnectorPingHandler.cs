using Common.Messaging.Commands;
using Connector.Hub.Api.Services;
using Messaging.Demo.Common.Commands;

namespace Connector.Hub.Api.Handlers;

public class ConnectorPingHandler(IConnectorHubManager connectionManager)
    : CommandHandlerBase<ConnectorPingCommand, ConnectorPingResponse>
{
    protected override async Task HandleMessage(ConnectorPingCommand command, CommandContext context, CancellationToken ct)
    {
        // This is an RPC style command, so we need to wait for the response from
        // the client before we can send a response back to the originating service.  
        var response = await connectionManager.SendCommandWaitResponseAsync(command.ConnectorId, context, command, ct);
        if (response is not null)
        {
            SetResponse(context, response);
        }
    }
}
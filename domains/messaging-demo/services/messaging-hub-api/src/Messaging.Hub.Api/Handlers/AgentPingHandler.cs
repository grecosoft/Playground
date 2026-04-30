using Common.Messaging.Commands;
using Messaging.Demo.Common.Commands;
using Messaging.Hub.Api.Services;

namespace Messaging.Hub.Api.Handlers;

public class AgentPingHandler(IConnectorHubManager connectionManager)
    : CommandHandlerBase<AgentPingCommand, AgentPingResponse>
{
    protected override async Task HandleMessage(AgentPingCommand command, CommandContext context, CancellationToken ct)
    {
        // This is an RPC style command, so we need to wait for the response from
        // the client before we can send a response back to the originating service.  
        var response = await connectionManager.SendCommandWaitResponseAsync(command.AgentId, context, command, ct);
        if (response is not null)
        {
            SetResponse(context, response);
        }
    }
}
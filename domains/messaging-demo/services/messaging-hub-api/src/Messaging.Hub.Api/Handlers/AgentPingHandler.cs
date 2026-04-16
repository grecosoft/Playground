using Common.Messaging.Commands;
using Messaging.Demo.Common.Commands;
using Messaging.Hub.Infra;
using Microsoft.AspNetCore.SignalR;

namespace Messaging.Hub.Api.Handlers;

public class AgentPingHandler(
    IHubContext<ConnectorHub> connectorHub) : CommandHandlerBase<AgentPingCommand, AgentPingResponse>
{
    protected override async Task HandleMessage(
        AgentPingCommand command,
        CommandContext context,
        CancellationToken cancellationToken)
    {
        var response = new AgentPingResponse(
            command.EchoMessage,
            Guid.NewGuid().ToString());
        
        context.SetResponse(response);
        
        await connectorHub.Clients
            .User(command.AgentId)
            .SendAsync(context.CommandNamespace, command, cancellationToken);
    }
}
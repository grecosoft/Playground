using Common.Messaging.Commands;
using Messaging.Demo.Common.Commands;
using Messaging.Hub.Infra;
using Microsoft.AspNetCore.SignalR;

namespace Messaging.Hub.Api.Handlers;

public class PingMessageHandler(
    IHubContext<ConnectorHub> connectorHub) : CommandHandlerBase<PingCommand, PingResponse>
{
    protected override async Task HandleMessage(
        PingCommand command,
        CommandContext context,
        CancellationToken cancellationToken)
    {

        context.SetResponse(new PingResponse($"Pong-{command.ValueOne}/{command.ValueOne}-{Guid.NewGuid()}"));
        await connectorHub.Clients.All.SendAsync("command-message", command.ValueOne, cancellationToken);
    }
}
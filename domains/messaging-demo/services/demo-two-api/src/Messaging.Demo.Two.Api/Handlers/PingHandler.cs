using Common.Messaging;
using Common.Messaging.Commands;
using Messaging.Demo.Common.Commands;

namespace Messaging.Demo.Two.Api.Handlers;

public class PingMessageHandler : CommandHandlerBase<PingCommand, PingResponse>
{
    protected override Task HandleMessage(
        PingCommand command,
        CommandContext context,
        CancellationToken cancellationToken)
    {

        context.SetResponse(new PingResponse($"Pong-{command.ValueOne}/{command.ValueOne}-{Guid.NewGuid()}"));
        return Task.CompletedTask;
    }
}
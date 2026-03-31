using Common.Messaging;
using Messaging.Demo.Common.Commands;

namespace Messaging.Demo.Two.Api.Handlers;

public class PingMessageHandler : CommandHandlerBase<PingCommand, PingResponse>
{
    protected override Task HandleMessage(
        PingCommand command,
        CommandContext<PingResponse> context,
        CancellationToken cancellationToken)
    {

        context.SetResponse(new PingResponse($"Pong-{command.ValueOne}/{command.ValueOne}-{Guid.NewGuid()}"));
        return Task.CompletedTask;
    }
}
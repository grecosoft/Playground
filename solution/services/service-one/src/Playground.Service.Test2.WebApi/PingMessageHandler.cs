using Playground.Common;
using Playground.Common.Messaging;

namespace Playground.Service.Test2.WebApi;

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
using Playground.Common;
using Playground.Common.Messaging;

namespace Playground.Service.Test.WebApi;

public class PingMessageHandler : CommandHandlerBase<PingCommand, PingResponse>
{
    protected override Task<PingResponse> HandleMessage(PingCommand command, CancellationToken cancellationToken)
    {
        return Task.FromResult(new PingResponse("Pong"));
    }
}
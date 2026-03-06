using Playground.Common.Messaging.Types;

namespace Playground.Common;

[CommandNamespace("datalab.commands.ping")]
public record PingCommand(string ExpectedValue) : ICommandMessage<PingResponse>
{
    
}

public record PingResponse(string ActualValue);
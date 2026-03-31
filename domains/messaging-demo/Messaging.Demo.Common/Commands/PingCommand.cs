using Common.Messaging.Entities;

namespace Messaging.Demo.Common.Commands;

[MessageNamespace("commands.ping")]
public record PingCommand(string ValueOne, string ValueTwo) : ICommandMessage<PingResponse>
{
    public PingResponse? Response { get; set; }
}

public record PingResponse(string PingValue);
using Playground.Common.Messaging.Types;

namespace Playground.Common;


[MessageNamespace("commands.ping")]
public record PingCommand(string ValueOne, string ValueTwo) : ICommandMessage<PingResponse>
{
    public PingResponse? Response { get; set; }
}

public record PingResponse(string PingValue);

[MessageNamespace("commands.device.status")]
public record DeviceUpdate(string DeviceId) : ICommandMessage<DeviceStatus>
{
    public DeviceStatus? Response { get; set; }
}

public record DeviceStatus(bool IsEnabled);
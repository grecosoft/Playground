using Common.Messaging.Entities;

namespace Messaging.Demo.Common.Commands;

[MessageNamespace("commands.device.status")]
public record DeviceUpdateCommand(string DeviceId) : ICommandMessage<DeviceStatus>
{
    public DeviceStatus? Response { get; set; }
}

public record DeviceStatus(
    bool IsEnabled,
    int BatteryLevel,
    bool IsCharging,
    string City,
    string State,
    string ZipCode);
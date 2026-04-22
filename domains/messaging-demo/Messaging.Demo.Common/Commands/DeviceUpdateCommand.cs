using System.Text.Json.Serialization;
using Common.Messaging.Entities;

namespace Messaging.Demo.Common.Commands;

[MessageNamespace("commands.device.status")]
public record DeviceUpdateCommand(
    [property: JsonPropertyName("deviceId")] string DeviceId) : ICommandMessage<DeviceStatus>
{
    public DeviceStatus? Response { get; set; }
}

public record DeviceStatus(
    [property: JsonPropertyName("isEnabled")] bool IsEnabled,
    [property: JsonPropertyName("batteryLevel")] int BatteryLevel,
    [property: JsonPropertyName("isCharging")] bool IsCharging,
    [property: JsonPropertyName("city")] string City,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("zipCode")] string ZipCode);
using System.Text.Json;
using Playground.Common;
using Playground.Common.Messaging;

namespace Playground.Service.Test.WebApi;

public class DeviceStatusHandler : CommandHandlerBase<DeviceUpdate, DeviceStatus>
{
    protected override Task HandleMessage(DeviceUpdate command,
        CommandContext<DeviceStatus> context,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"{nameof(DeviceUpdate)} received: {JsonSerializer.Serialize(command)}");
        
        return Task.CompletedTask;
    }
}
using System.Text.Json;
using Common.Messaging.Commands;
using Messaging.Demo.Common.Commands;

namespace Messaging.Demo.One.Api.Handlers;

public class DeviceStatusHandler : CommandHandlerBase<DeviceUpdateCommand, DeviceStatus>
{
    protected override Task HandleMessage(DeviceUpdateCommand command,
        CommandContext context,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"{nameof(DeviceUpdateCommand)} received: {JsonSerializer.Serialize(command)}");
        return Task.CompletedTask;
    }
}
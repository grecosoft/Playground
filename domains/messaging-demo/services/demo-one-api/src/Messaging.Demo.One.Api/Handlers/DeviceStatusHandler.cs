using System.Text.Json;
using Common.Messaging.Commands;
using Messaging.Demo.Common.Commands;

namespace Messaging.Demo.One.Api.Handlers;

public class DeviceStatusHandler(
    ILogger<DeviceStatusHandler> logger) 
    : CommandHandlerBase<DeviceUpdateCommand, DeviceStatus>
{
    protected override Task HandleMessage(DeviceUpdateCommand command,
        CommandContext context,
        CancellationToken cancellationToken)
    {
        logger.LogWarning("Response Message: {message}", JsonSerializer.Serialize(command));
        return Task.CompletedTask;
    }
}
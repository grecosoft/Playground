using System.Text.Json;
using Common.Messaging.Commands;
using Messaging.Demo.Common.Commands;
using Serilog;

namespace Messaging.Demo.One.Api.Handlers;

public class DeviceStatusHandler() 
    : CommandHandlerBase<DeviceUpdateCommand, DeviceStatus>
{
    protected override Task HandleMessage(DeviceUpdateCommand command,
        CommandContext context,
        CancellationToken cancellationToken)
    {
        Log.Logger.Warning("Response Message: {message}", JsonSerializer.Serialize(command));
        return Task.CompletedTask;
    }
}
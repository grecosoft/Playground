using Common.Messaging;
using Messaging.Demo.Common.Commands;

namespace Messaging.Demo.Two.Api.Handlers;

public class DeviceStatusHandler : CommandHandlerBase<DeviceUpdateCommand, DeviceStatus>
{
    protected override async Task HandleMessage(DeviceUpdateCommand command,
        CommandContext<DeviceStatus> context,
        CancellationToken cancellationToken)
    {
        await context.SaveCommandAsync(cancellationToken);
    }
}
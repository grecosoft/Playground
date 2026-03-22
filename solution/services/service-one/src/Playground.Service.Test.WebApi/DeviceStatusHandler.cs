using Playground.Common;
using Playground.Common.Messaging;

namespace Playground.Service.Test.WebApi;

public class DeviceStatusHandler : CommandHandlerBase<DeviceUpdate, DeviceStatus>
{
    protected override async Task HandleMessage(DeviceUpdate command,
        CommandContext<DeviceStatus> context,
        CancellationToken cancellationToken)
    {
        await context.SaveCommandAsync(cancellationToken);
    }
}
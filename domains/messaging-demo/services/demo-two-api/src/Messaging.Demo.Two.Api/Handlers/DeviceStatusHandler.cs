// using Common.Messaging.Commands;
// using Messaging.Demo.Common.Commands;
//
// namespace Messaging.Demo.Two.Api.Handlers;
//
// public class DeviceStatusHandler : CommandHandlerBase<DeviceUpdateCommand, DeviceStatus>
// {
//     protected override async Task HandleMessage(DeviceUpdateCommand command,
//         CommandContext context,
//         CancellationToken cancellationToken)
//     {
//         await context.CommandRepository.SaveCommand(context, cancellationToken);
//     }
// }
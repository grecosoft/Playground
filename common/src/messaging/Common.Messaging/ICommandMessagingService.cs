using Common.Messaging.Commands;
using Common.Messaging.Entities;

namespace Common.Messaging;

public interface ICommandMessagingService
{
    Task SendResponseToCommandAsync<TResponse>(
        CommandContext commandContext,
        ICommandMessage<TResponse> command,
        CancellationToken token);

}
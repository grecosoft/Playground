using Common.Messaging.Entities;

namespace Common.Messaging;

public interface IMessagingService
{
    Task SendResponseToCommandAsync<TResponse>(
        ReceivedCommand receivedCommand,
        ICommandMessage<TResponse> command,
        CancellationToken token);

}
using Playground.Common.Messaging.Services;
using Playground.Common.Messaging.Types;

namespace Playground.Common.Messaging;

public interface IMessagingService
{
    Task SendResponseToCommandAsync<TResponse>(
        ReceivedCommand receivedCommand,
        ICommandMessage<TResponse> command,
        CancellationToken token);

}
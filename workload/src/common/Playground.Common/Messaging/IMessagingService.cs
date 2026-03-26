using Playground.Common.Messaging.Services;
using Playground.Common.Messaging.Types;

namespace Playground.Common.Messaging;

public interface IMessagingService
{
    
    Task<TResponse> SendCommandWithReplyAsync<TResponse>(
        ICommandMessage<TResponse> command,
        CancellationToken token);
    
    Task SendCommandAsync<TResponse>(
        ICommandMessage<TResponse> command,
        CancellationToken token);

    Task SendResponseToCommandAsync<TResponse>(
        ReceivedCommand receivedCommand,
        ICommandMessage<TResponse> command,
        CancellationToken token);

}
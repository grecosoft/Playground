using Playground.Common.Messaging.Types;

namespace Playground.Common.Messaging;

public interface IMessagingService
{
    
    Task<TResponse> SendCommandWithReplyAsync<TResponse>(ICommandMessage<TResponse> command, CancellationToken token);
    Task SendCommandWithResponseAsync<TResponse>(ICommandMessage<TResponse> command, CancellationToken token);

}
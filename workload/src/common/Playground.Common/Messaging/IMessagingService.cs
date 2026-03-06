using Playground.Common.Messaging.Types;

namespace Playground.Common.Messaging;

public interface IMessagingService
{
    Task<TResponse> SendAsync<TResponse>(ICommandMessage<TResponse> command, CancellationToken token);

    Task SendAsync(ICommandMessage command, CancellationToken token);

}
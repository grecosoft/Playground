using Playground.Common.Messaging.Types;

namespace Playground.Common.Messaging;

public record EndpointInfo(
    string ServiceName,
    Guid ServiceId);

public interface IServiceEndpoint
{
    public EndpointInfo EndpointInfo { get; }
    
    Task<TResponse> SendCommandWithReplyAsync<TResponse>(
        ICommandMessage<TResponse> command,
        CancellationToken token);
    
    public Task SendCommandAsync<TResponse>(
        ICommandMessage<TResponse> command,
        CancellationToken token);
}

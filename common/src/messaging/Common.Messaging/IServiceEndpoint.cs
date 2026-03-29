using Common.Messaging.Contracts;

namespace Common.Messaging;

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

using Common.Messaging.Entities;

namespace Common.Messaging;

public record EndpointInfo(
    string ServiceName,
    Guid ServiceId);

public interface ICommandEndpoint
{
    public EndpointInfo EndpointInfo { get; }
    
    Task<TResponse> SendCommandWithReplyAsync<TResponse>(
        ICommandMessage<TResponse> command,
        CancellationToken token);
    
    public Task SendCommandAsync<TResponse>(
        ICommandMessage<TResponse> command,
        CancellationToken token);
}

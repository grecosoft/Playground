using Common.Messaging.Entities;

namespace Common.Messaging.Core.Commands;

public class CommandEndpoint(
    EndpointInfo endpointInfo,
    CommandMessaging messaging): ICommandEndpoint
{
    public EndpointInfo EndpointInfo { get; } = endpointInfo;
    
    public Task<TResponse> SendCommandWithReplyAsync<TResponse>(
        ICommandMessage<TResponse> command,
        CancellationToken token) => messaging.SendCommandWithReplyAsync<TResponse>(command, EndpointInfo, token);

    public Task SendCommandAsync<TResponse>(
        ICommandMessage<TResponse> command,
        CancellationToken token) => messaging.SendCommandAsync(command, EndpointInfo, token);
}


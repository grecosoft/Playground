using Common.Messaging.Entities;

namespace Common.Messaging.Core.Commands;

public class CommandEndpoint(
    EndpointInfo endpointInfo,
    CommandMessagingService messagingService): ICommandEndpoint
{
    public EndpointInfo EndpointInfo { get; } = endpointInfo;
    
    public Task<TResponse> SendCommandWithReplyAsync<TResponse>(
        ICommandMessage<TResponse> command,
        CancellationToken token) => messagingService.SendCommandWithReplyAsync<TResponse>(command, EndpointInfo, token);

    public Task SendCommandAsync<TResponse>(
        ICommandMessage<TResponse> command,
        CancellationToken token) => messagingService.SendCommandAsync(command, EndpointInfo, token);
}


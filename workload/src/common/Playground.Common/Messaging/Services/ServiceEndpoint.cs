using Microsoft.Extensions.Logging;
using Playground.Common.Messaging.Types;

namespace Playground.Common.Messaging.Services;

public class ServiceEndpoint(
    ILogger logger,
    EndpointInfo endpointInfo,
    MessagingService messagingService): IServiceEndpoint
{
    public EndpointInfo EndpointInfo { get; } = endpointInfo;
    
    public Task<TResponse> SendCommandWithReplyAsync<TResponse>(
        ICommandMessage<TResponse> command,
        CancellationToken token)
    {
        return messagingService.SendCommandWithReplyAsync<TResponse>(command, EndpointInfo, token);
    }

    public Task SendCommandAsync<TResponse>(
        ICommandMessage<TResponse> command,
        CancellationToken token)
    {
        return messagingService.SendCommandAsync(command, EndpointInfo, token);
    }
}


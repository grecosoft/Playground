using Common.Messaging.Entities;
using Microsoft.Extensions.Logging;

namespace Common.Messaging.Core;

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
        logger.LogInformation("Sending command of type {CommandType}", command.GetType().FullName);
        return messagingService.SendCommandWithReplyAsync<TResponse>(command, EndpointInfo, token);
    }

    public Task SendCommandAsync<TResponse>(
        ICommandMessage<TResponse> command,
        CancellationToken token)
    {
        return messagingService.SendCommandAsync(command, EndpointInfo, token);
    }
}


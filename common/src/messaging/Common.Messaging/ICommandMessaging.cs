using Common.Messaging.Commands;
using Common.Messaging.Entities;

namespace Common.Messaging;

public interface ICommandMessaging
{
    ICommandEndpoint GetServiceEndpoint(string serviceName);
    
    Task SendResponseToCommandAsync<TResponse>(
        CommandContext commandContext,
        ICommandMessage<TResponse> command,
        CancellationToken token);
}
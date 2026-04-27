using Common.Messaging.Entities;

namespace Common.Messaging.Core.Commands;

/// <summary>
/// Created when the messaging services are added to the dependency injection container via the
/// AddBusMessaging extension method.  This class is responsible for sending commands to a specific endpoint.
/// </summary>
/// <param name="endpointInfo">Information used to identity the service endpoint in code and at runtime.</param>
/// <param name="messaging">The core command messaging implementation to which the class delegates for a
/// specific service endpoint.</param>
public class CommandEndpoint(
    EndpointInfo endpointInfo,
    CommandMessaging messaging): ICommandEndpoint
{
    public EndpointInfo EndpointInfo { get; } = endpointInfo;
    
    public async Task<CommandResult<TResponse>> SendCommandWithReplyAsync<TResponse>(
        ICommandMessage<TResponse> command,
        CancellationToken ct,
        CommandOptions? options = null)
    {
        options ??= new CommandOptions();
        
        var result = await messaging.SendCommandWithReplyAsync<TResponse>(command, EndpointInfo, ct);
        if (options.ThrowIfErrorResponse && !string.IsNullOrWhiteSpace(result.ErrorMessage))
        {
            throw new CommandResultException(result.ErrorMessage);
        }

        return result;
    }

    public Task SendCommandAsync<TResponse>(
        ICommandMessage<TResponse> command,
        CancellationToken ct) => messaging.SendCommandAsync(command, EndpointInfo, ct);
}


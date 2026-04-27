using Common.Messaging.Entities;

namespace Common.Messaging;

/// <summary>
/// Information about a dependent service to which the executing service can send commands.
/// </summary>
/// <param name="ServiceName">The name representing the service.</param>
/// <param name="ServiceId">The unique identity of the service.
/// </param>
public record EndpointInfo(
    string ServiceName,
    string ServiceId);

/// <summary>
/// Interface representing a service to which a command can be sent.  When the AddBusMessaging extension
/// method is used to register messaging services, an implementation of this interface will is automatically created
/// for each service endpoint defined in the configuration.
/// </summary>
public interface ICommandEndpoint
{
    /// <summary>
    /// Details about the endpoint.  Most importantly, the GUID representing the dependent service to which
    /// commands can be sent.  The Terraform service_messaging module automatically creates subscriptions on
    /// the command topic so the dependent service will receive command messages sent to this endpoint.
    /// </summary>
    public EndpointInfo EndpointInfo { get; }

    /// <summary>
    /// Used to send a command to the endpoint and wait for a specified number of seconds for a response.
    /// </summary>
    /// <param name="command">The command to be sent.</param>
    /// <param name="ct">Cancellation Token</param>
    /// <param name="options">Options used when sending command or evaluating its results.</param>
    /// <typeparam name="TResponse">The response inferred from the command.</typeparam>
    /// <returns>The response of the command.</returns>
    Task<CommandResult<TResponse>> SendCommandWithReplyAsync<TResponse>(
        ICommandMessage<TResponse> command,
        CancellationToken ct,
        CommandOptions? options = null);
    
    /// <summary>
    /// Used to send a command to the endpoint and does not wait for an immediate response. Once the response of the
    /// command is known by the dependent service, it sends the response back to the originating service by calling
    /// the method named SendResponseToCommandAsync defined on ICommandMessaging. 
    /// </summary>
    /// <param name="command">The command to be sent.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <typeparam name="TResponse">The response type of the command inferred from the command.</typeparam>
    /// <returns>Future result.</returns>
    public Task SendCommandAsync<TResponse>(
        ICommandMessage<TResponse> command,
        CancellationToken ct);
}

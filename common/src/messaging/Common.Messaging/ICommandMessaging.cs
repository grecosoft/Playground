using Common.Messaging.Commands;
using Common.Messaging.Entities;

namespace Common.Messaging;

/// <summary>
/// Interface for resolving a service endpoint by name or sending responses to previously received commands
/// for which the caller knows the identity of the service to which a response should be sent.
/// </summary>
public interface ICommandMessaging
{
    /// <summary>
    /// Used to obtains a service's endpoint to which commands can be sent.
    /// </summary>
    /// <param name="serviceName">The name of the service.</param>
    /// <returns>Reference to the endpoint or an exception if not found.</returns>
    ICommandEndpoint GetServiceEndpoint(string serviceName);
    
    /// <summary>
    /// Used by a service to send a response to a previously received command.  This method would typically be used
    /// within a service that receives commands, and needs to send a response back to the caller of the command.
    /// The caller of the command is not expected to be waiting for an immediate response, but this method allows the
    /// service to send a response back to the caller once the response is known.
    /// </summary>
    /// <param name="commandContext">Describes the service from which the original command was received.
    /// This includes the identity of the service and the correlation id of the original command.</param>
    /// <param name="command">The command to be sent back to the originating service containing the response.</param>
    /// <param name="tc">The cancellation token.</param>
    /// <typeparam name="TResponse">The type of the response associated with the command.</typeparam>
    /// <returns>Future result.</returns>
    Task SendResponseToCommandAsync<TResponse>(
        CommandContext commandContext,
        ICommandMessage<TResponse> command,
        CancellationToken tc);
}
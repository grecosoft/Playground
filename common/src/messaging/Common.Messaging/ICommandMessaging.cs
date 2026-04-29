using Common.Messaging.Commands;

namespace Common.Messaging;

/// <summary>
/// Options that can be passed to determine how a command is sent and how its response is handled.
/// </summary>
/// <param name="ThrowIfErrorResponse">For a command for which a response is awaited, determines
/// if an exception should be thrown if the response indicates an error.</param>
public record CommandOptions(bool ThrowIfErrorResponse = false);

/// <summary>
/// Returned for an awaited command's response.
/// </summary>
/// <param name="Response">The response of the command.</param>
/// <param name="ErrorMessage">Error messaged returned by service if handling command resulted in error.</param>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public record CommandResult<TResponse>(TResponse? Response, string? ErrorMessage = null);

/// <summary>
/// Exception thrown if the service processing the command resulted in an exception.
/// </summary>
/// <param name="message">The error message returned by the service.</param>
public class CommandResultException(string message) : Exception(message);

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
    /// <param name="tc">The cancellation token.</param>
    /// <returns>Future result.</returns>
    Task SendResponseToCommandAsync(
        CommandContext commandContext,
        CancellationToken tc);
}
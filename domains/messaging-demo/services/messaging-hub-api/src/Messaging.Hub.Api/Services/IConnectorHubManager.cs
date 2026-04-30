using Common.Messaging.Commands;
using Common.Messaging.Entities;

namespace Messaging.Hub.Api.Services;

public interface IConnectorHubManager
{
    /// <summary>
    /// Called when an external Cloud Connector connects to the SignalR Hub.
    /// </summary>
    /// <param name="connectorId">The identity of the connecting connector.</param>
    /// <param name="connectionId">The SignalR connection id assocated with the connector.</param>
    void AddConnection(string connectorId, string connectionId);

    /// <summary>
    /// Called when an external Cloud Connector disconnects from the SignalR Hub.
    /// </summary>
    /// <param name="connectorId">The identity of the connector that disconnected.</param>
    /// <param name="connectionId">The SignalR connection id associated with the disconnected connector.</param>
    void RemoveConnection(string connectorId, string connectionId);
    
    /// <summary>
    /// Returns the connection id for a given connector.
    /// </summary>
    /// <param name="connectorId"></param>
    /// <returns>The connection id associated when the connector.  If the connector is currently not
    /// connected, null is returned.</returns>
    string? GetConnection(string connectorId);

    /// <summary>
    /// Sends command to a connected Cloud Connector and waits a specified about of time for a response.
    /// </summary>
    /// <param name="connectorId">The identity of the connector to send command.</param>
    /// <param name="context">The current context of the received command from an internal service.</param>
    /// <param name="command">The command to send.</param>
    /// <param name="ct">Calculation token.</param>
    /// <typeparam name="TResponse">The command's response type.</typeparam>
    /// <returns>Response if valid. Otherwise, null.</returns>
    Task<TResponse?> SendCommandWaitResponseAsync<TResponse>(
        string connectorId,
        CommandContext context,
        ICommandMessage<TResponse> command,
        CancellationToken ct);

    /// <summary>
    /// Sends command to a connected Cloud Connector and does not wait for a response.  The Connector will respond
    /// back to the hub with a matching correlation ID when the response is ready, which is then routed back to the
    /// originating service.
    /// </summary>
    /// <param name="connectorId">The identity of the connector to send command.</param>
    /// <param name="context">The current context of the received command from an internal service.</param>
    /// <param name="command">The command to send.</param>
    /// <param name="ct">Calculation token.</param>
    /// <typeparam name="TResponse">The command's response type.</typeparam>
    /// <returns>Future result.</returns>
    Task SendCommandFutureResponseAsync<TResponse>(
        string connectorId,
        CommandContext context,
        ICommandMessage<TResponse> command,
        CancellationToken ct);
}
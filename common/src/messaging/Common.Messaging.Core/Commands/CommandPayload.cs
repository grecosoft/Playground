namespace Common.Messaging.Core.Commands;

/// <summary>
/// Data structure representing the payload of a command passed between the sending and receiving service.
/// </summary>
/// <param name="SendingServiceId">The identity of the service sending the command.</param>
/// /// <param name="SendingServiceName">A friendly name for the service sending the command used for logging.</param>
/// <param name="Command">The data of the command sent from the calling service.</param>
/// <param name="Response">The data of the command's response sent from the receiving service.</param>
/// /// <param name="ResponseError">Error details if processing the response resulted in an error.</param>
public record CommandPayload(
    string SendingServiceId,
    string SendingServiceName,
    BinaryData Command,
    BinaryData Response,
    string? ResponseError = null)
{
    /// <summary>
    /// Indicates if the command's response data was set.
    /// </summary>
    public bool HasResponse => !Response.IsEmpty;
}
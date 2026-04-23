namespace Common.Messaging.Core.Commands;

/// <summary>
/// Data structure representing the payload of a command passed between the sending and receiving service.
/// </summary>
/// <param name="Command">The data of the command sent from the calling service.</param>
/// <param name="Response">The data of the command's response sent from the receiving service.</param>
public record CommandPayload(
    BinaryData Command,
    BinaryData Response)
{
    /// <summary>
    /// Indicates if the command's response data was set.
    /// </summary>
    public bool HasResponse => !Response.IsEmpty;
}
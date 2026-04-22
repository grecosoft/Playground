namespace Common.Messaging.Core.Commands;

public record CommandPayload(
    BinaryData Command,
    BinaryData Response)
{
    public bool HasResponse => !Response.IsEmpty;
}
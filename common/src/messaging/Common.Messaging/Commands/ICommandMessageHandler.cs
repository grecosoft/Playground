namespace Common.Messaging.Commands;

/// <summary>
/// Interface implemented by classes responsible for handling received commands.
/// </summary>
public interface ICommandMessageHandler
{
    /// <summary>
    /// The type of the command the handler is responsible for handling.
    /// </summary>
    public Type CommandType { get; }
    
    /// <summary>
    /// The optional response type of the command.
    /// </summary>
    public Type? ResponseType { get; }

    /// <summary>
    /// Invoked when a command is received.
    /// </summary>
    /// <param name="commandContext">Context containing information about the received command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns></returns>
    public Task Handle(CommandContext commandContext, CancellationToken ct);
}
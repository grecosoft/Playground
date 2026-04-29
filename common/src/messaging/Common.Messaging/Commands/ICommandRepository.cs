using Common.Messaging.Entities;

namespace Common.Messaging.Commands;

/// <summary>
/// Repository for saving received commands and their associated state.  Used to store commands by the receiving
/// service until a response is known and can be sent back to the caller of the command, at which point the command
/// can be deleted from the repository.
/// </summary>
public interface ICommandRepository
{
    /// <summary>
    /// Save a received command and its associated metadata so a response can be sent back to the caller of the
    /// command at a later time when the response is known.
    /// </summary>
    /// <param name="commandContext">The command and associated metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The identity of the saved record.</returns>
    Task<Guid> SaveCommandContext(CommandContext commandContext, CancellationToken ct);

    /// <summary>
    /// Loads a previously saved command context and deserializes the command to the specified type.
    /// </summary>
    /// <param name="correlationId">The correlation id of the received command.</param>
    /// <param name="ct">Cancellation Context.</param>
    /// <typeparam name="T">The type of the command.</typeparam>
    /// <returns>Command context containing the original command and associated metadata.</returns>
    Task<CommandContext> LoadTypedCommandContext<T>(string correlationId, CancellationToken ct)
        where T : ICommandMessage;

    /// <summary>
    /// Loads a previously saved command context without deserializing the command.  This can be used in scenarios
    /// where the command needs to be accessed but the type of the command is not statically available. 
    /// </summary>
    /// <param name="correlationId">The correlation id of the received command.</param>
    /// <param name="ct">Cancellation Context.</param>
    /// <returns>Command context containing the original command and associated metadata.</returns>
    Task<CommandContext?> LoadCommandContext(string correlationId, CancellationToken ct);
    
    /// <summary>
    /// Deletes a previously save command context.
    /// </summary>
    /// <param name="correlationId">The correlation id of the received command.</param>
    /// <param name="ct">Cancellation Context.</param>
    /// <returns>Future Result</returns>
    Task DeleteCommandCommand(string correlationId, CancellationToken ct);
    
    /// <summary>
    /// Returns a list of commands pending responds back to the calling service.
    /// </summary>
    /// <param name="ct">Cancellation Token</param>
    /// <returns>List of pending commands.</returns>
    Task<IEnumerable<CommandContext>> GetPendingCommandContextsAsync(CancellationToken ct);
}
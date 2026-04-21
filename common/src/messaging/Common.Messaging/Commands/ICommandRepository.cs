using Common.Messaging.Entities;

namespace Common.Messaging.Commands;

public interface ICommandRepository
{
    Task SaveCommand(CommandContext commandContext, CancellationToken cancellationToken);

    Task<CommandContext> LoadCommand<T>(string correlationId, CancellationToken cancellationToken)
        where T : ICommandMessage;

    Task DeleteCommand(string correlationId, CancellationToken cancellationToken);
    
    Task<IEnumerable<CommandContext>> GetPendingCommandsAsync(CancellationToken cancellationToken);
}
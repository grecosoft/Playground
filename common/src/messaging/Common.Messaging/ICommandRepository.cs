using Common.Messaging.Entities;

namespace Common.Messaging;

public interface ICommandRepository
{
    Task SaveCommand(ReceivedCommand receivedCommand, CancellationToken cancellationToken);

    Task<ReceivedCommand> LoadCommand<T>(string correlationId, CancellationToken cancellationToken)
        where T : ICommandMessage;
}
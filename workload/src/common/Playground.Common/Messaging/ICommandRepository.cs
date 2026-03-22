using Playground.Common.Messaging.Services;
using Playground.Common.Messaging.Types;

namespace Playground.Common.Messaging;

public interface ICommandRepository
{
    Task SaveCommand(ReceivedCommand receivedCommand, CancellationToken cancellationToken);

    Task<ReceivedCommand> LoadCommand<T>(string correlationId, CancellationToken cancellationToken)
        where T : ICommandMessage;
}
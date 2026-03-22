using Playground.Common.Messaging.Services;

namespace Playground.Common.Messaging;

public interface ICommandMessageHandler
{
    public Type CommandType { get; }
    public Type? ResponseType { get; }

    public Task<CommandContext> Handle(
        ReceivedCommand command,
        ICommandRepository commandRepository,
        CancellationToken cancellationToken);
}


    
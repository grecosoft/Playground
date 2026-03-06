using Playground.Common.Messaging.Types;

namespace Playground.Common.Messaging;

public interface ICommandMessageHandler
{
    public Type CommandType { get; }
    public Type? ResponseType { get; }

    public Task<object?> Handle(ICommandMessage command, CancellationToken cancellationToken);
}


    
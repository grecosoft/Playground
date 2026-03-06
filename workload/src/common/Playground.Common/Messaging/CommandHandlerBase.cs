using Playground.Common.Messaging.Types;

namespace Playground.Common.Messaging;

public abstract class CommandHandlerBase<TCommand> : ICommandMessageHandler
    where TCommand : ICommandMessage
{
    public Type CommandType { get; } = typeof(TCommand);
    public Type? ResponseType { get; } = null;
    
    public async Task<object?> Handle(ICommandMessage command, CancellationToken cancellationToken)
    {
        await HandleMessage((TCommand)command, cancellationToken);
        return null;
    }
    
    protected abstract Task HandleMessage(TCommand command, CancellationToken cancellationToken);
}

public abstract class CommandHandlerBase<TCommand, TResponse> : ICommandMessageHandler
    where TCommand : ICommandMessage<TResponse> 
{
    public Type CommandType { get; } = typeof(TCommand);
    public Type? ResponseType { get; } =  typeof(TResponse);
    
    public async Task<object?> Handle(ICommandMessage command, CancellationToken cancellationToken)
    {
        return await HandleMessage((TCommand)command, cancellationToken);
    }

    protected abstract Task<TResponse> HandleMessage(TCommand command, CancellationToken cancellationToken);
}
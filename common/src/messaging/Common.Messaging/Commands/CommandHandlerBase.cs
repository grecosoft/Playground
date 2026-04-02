using Common.Messaging.Entities;

namespace Common.Messaging;

public abstract class CommandHandlerBase<TCommand, TResponse> : ICommandMessageHandler
    where TCommand : ICommandMessage<TResponse> 
{
    public Type CommandType { get; } = typeof(TCommand);
    public Type? ResponseType { get; } =  typeof(TResponse);

    public async Task Handle(CommandContext commandContext, CancellationToken cancellationToken)
    {
        await HandleMessage((TCommand)commandContext.Command, commandContext, cancellationToken);
    }
    
    protected abstract Task HandleMessage(TCommand command, CommandContext context, CancellationToken cancellationToken);
}
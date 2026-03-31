using Common.Messaging.Entities;

namespace Common.Messaging;

public abstract class CommandHandlerBase<TCommand, TResponse> : ICommandMessageHandler
    where TCommand : ICommandMessage<TResponse> 
{
    public Type CommandType { get; } = typeof(TCommand);
    public Type? ResponseType { get; } =  typeof(TResponse);

    public async Task<CommandContext> Handle(
        ReceivedCommand receivedCommand, 
        ICommandRepository commandRepository,
        CancellationToken cancellationToken)
    {
        var commandContext = new CommandContext<TResponse>(receivedCommand, commandRepository);
        
        await HandleMessage((TCommand)receivedCommand.Command, commandContext, cancellationToken);
        
        return commandContext;
    }
    
    protected abstract Task HandleMessage(
        TCommand command, 
        CommandContext<TResponse> context,
        CancellationToken cancellationToken);
}
using Common.Messaging.Entities;

namespace Common.Messaging.Commands;

public abstract class CommandHandlerBase<TCommand, TResponse> : ICommandMessageHandler
    where TCommand : ICommandMessage<TResponse> 
{
    public Type CommandType { get; } = typeof(TCommand);
    public Type? ResponseType { get; } =  typeof(TResponse);

    public async Task Handle(CommandContext commandContext, CancellationToken cancellationToken)
    {
        var command = (TCommand)commandContext.Command;
        if (commandContext.Response is not null)
        {
            command.Response = (TResponse)commandContext.Response;
        }
        
        await HandleMessage(command, commandContext, cancellationToken);
    }
    
    protected abstract Task HandleMessage(TCommand command, CommandContext context, CancellationToken cancellationToken);
}
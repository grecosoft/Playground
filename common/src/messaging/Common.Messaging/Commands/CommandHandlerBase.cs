using Common.Messaging.Entities;

namespace Common.Messaging.Commands;

/// <summary>
/// Base class from which specific command handlers are derived to processed received commands.
/// </summary>
/// <typeparam name="TCommand">The type of the command.</typeparam>
/// <typeparam name="TResponse">The command's response type.</typeparam>
public abstract class CommandHandlerBase<TCommand, TResponse> : ICommandMessageHandler
    where TCommand : ICommandMessage<TResponse> 
{
    public Type CommandType { get; } = typeof(TCommand);
    public Type? ResponseType { get; } =  typeof(TResponse);

    public async Task Handle(CommandContext commandContext, CancellationToken ct)
    {
        var command = (TCommand)commandContext.Command;
        if (commandContext.Response is not null)
        {
            command.Response = (TResponse)commandContext.Response;
        }

        // Invoke derived command handler method.
        await HandleMessage(command, commandContext, ct);
    }

    /// <summary>
    /// Invoked by derived command handler to set command's response.
    /// </summary>
    /// <param name="context">The current context of the received command.</param>
    /// <param name="response">The commands associated response.</param>
    protected void SetResponse(CommandContext context, TResponse response)
    {
        if (response is null)
        {
            throw new ArgumentNullException(nameof(response));
        }
        
        context.SetResponse(response);
    }
    
    /// <summary>
    /// Overridden by derived command handler to process received command.
    /// </summary>
    /// <param name="command">The received command.</param>
    /// <param name="context">Metadata about the received command.</param>
    /// <param name="ct">Cancellation Token.</param>
    /// <returns>Future Result once processing has completed.</returns>
    protected abstract Task HandleMessage(TCommand command, CommandContext context, CancellationToken ct);
}
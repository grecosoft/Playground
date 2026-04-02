namespace Common.Messaging;

public interface ICommandMessageHandler
{
    public Type CommandType { get; }
    public Type? ResponseType { get; }

    public Task Handle(CommandContext commandContext, CancellationToken cancellationToken);
}
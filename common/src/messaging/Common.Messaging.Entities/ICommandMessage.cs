namespace Common.Messaging.Entities;

public interface ICommandMessage: IMessage
{
    
}

public interface ICommandMessage<out TResponse> : ICommandMessage
{
    TResponse? Response { get; }
}
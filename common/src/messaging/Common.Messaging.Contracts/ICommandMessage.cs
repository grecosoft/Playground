namespace Common.Messaging.Contracts;

public interface ICommandMessage: IMessage
{
    
}

public interface ICommandMessage<out TResponse> : ICommandMessage
{
    TResponse? Response { get; }
}
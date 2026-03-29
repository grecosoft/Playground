namespace Playground.Common.Messaging.Types;

public interface IMessage
{
    
}

public interface ICommandMessage: IMessage
{
    
}


public interface ICommandMessage<out TResponse> : ICommandMessage
{
    TResponse? Response { get; }
}


public enum DispatchStrategyType
{
    Rpc = 1,
    Async = 2
}


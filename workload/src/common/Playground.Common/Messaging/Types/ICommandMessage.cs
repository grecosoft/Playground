namespace Playground.Common.Messaging.Types;

public interface IMessage
{
    
}

public interface ICommandMessage: IMessage
{
    
}


public interface ICommandMessage<TResponse> : ICommandMessage
{
    
}


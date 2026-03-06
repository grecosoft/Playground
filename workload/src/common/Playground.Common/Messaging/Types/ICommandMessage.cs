namespace Playground.Common.Messaging.Types;

public interface ICommandMessage
{
    
}


public interface ICommandMessage<TResponse> : ICommandMessage
{
    
}


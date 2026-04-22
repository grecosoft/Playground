namespace Common.Messaging.Entities;

/// <summary>
/// References a command without regards to it type of response.
/// </summary>
public interface ICommandMessage: IMessage
{
    
}

/// <summary>
/// Defines a command and it associated response type.
/// </summary>
/// <typeparam name="TResponse">The expected immediate or future response.</typeparam>
public interface ICommandMessage<TResponse> : ICommandMessage
{
    TResponse? Response { get; set; }
}
namespace Playground.Common.Messaging;

public class BusMessagingConfig
{
    public string FullyQualifiedNamespace { get; set; } = string.Empty;

    public int ReplyTimeoutSeconds { get; set; } = 50;
    
    /// <summary>
    /// The name of the queue that this service subscribes to for requests made by other services.
    /// </summary>
    public string CommandTopic { get; set; } = string.Empty;
    
    /// <summary>
    /// The queue this service subscribes for receiving replies to the originating message
    /// send to another service.
    /// </summary>
    public string ReplyQueue { get; set; } =  string.Empty;
}

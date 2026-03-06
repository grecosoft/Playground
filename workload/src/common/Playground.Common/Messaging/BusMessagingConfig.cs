namespace Playground.Common.Messaging;

public class BusMessagingConfig
{
    public string FullyQualifiedNamespace { get; set; } = string.Empty;

    public int ReplyTimeoutSeconds { get; set; } = 50;
    
    /// <summary>
    /// The name of the queue that this service subscribes to for requests made by other services.
    /// </summary>
    public string RequestQueue { get; set; } = string.Empty;
    
    /// <summary>
    /// The queue this service subscribes for receiving replies to the originating message
    /// send to another service.
    /// </summary>
    public string ReplyQueue { get; set; } =  string.Empty;
    
    /// <summary>
    /// The dependent services to which this service can send commands
    /// </summary>
    public Dictionary<string, DependentService> DependentServices { get; set; } = new();
}

public class DependentService
{
    public string RequestQueue { get; set; } = string.Empty;
    public string ReplyQueue { get; set; } =  string.Empty;
}
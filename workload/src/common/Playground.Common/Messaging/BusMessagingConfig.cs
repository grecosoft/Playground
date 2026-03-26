namespace Playground.Common.Messaging;

public class BusMessagingConfig
{
    public string ServiceId { get; set; } = string.Empty;
    
    public string FullyQualifiedNamespace { get; set; } = string.Empty;
    
    public Dictionary<string, DependentServiceOptions> DependentServices { get; init; } = [];

    /// <summary>
    /// The name of the topic used to send RPC style commands between a solution's services.
    /// </summary>
    public string SolutionCommandTopic { get; set; } = string.Empty;
    
    /// <summary>
    /// The name of the reply queue used to send responses to received RPC style commands back
    /// to the originating solution's service.  
    /// </summary>
    public string SolutionReplyQueue { get; set; } =  string.Empty;
    
    public int ReplyTimeoutSeconds { get; set; } = 50;
}

public class DependentServiceOptions
{
    public string Name { get; init; } = string.Empty;
    public Guid Id { get; init; }
}

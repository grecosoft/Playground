namespace Playground.Common.Messaging;

public class BusMessagingConfig
{
    public string SolutionName { get; set; } = string.Empty;
    
    public string ServiceName { get; set; } = string.Empty;
    
    public string FullyQualifiedNamespace { get; set; } = string.Empty;

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

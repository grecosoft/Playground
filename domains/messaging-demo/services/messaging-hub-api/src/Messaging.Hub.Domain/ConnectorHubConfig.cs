namespace Messaging.Hub.Domain;

public class ConnectorHubConfig
{ 
    public string SignalREndpoint { get; set; } = string.Empty;
    public string CommandSchemaRootPath { get; set; } = string.Empty;
    public int RpcReplyTimeoutSeconds { get; set; } = 5;
    
    public object ToLoggableProperties() => new
    {
        SignalREndpoint,
        CommandSchemaRootPath,
        RpcReplyTimeoutSeconds
    };
}
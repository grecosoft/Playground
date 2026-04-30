namespace CSharp.Connector;

public class AgentConfig
{
    public string ConnectorHubApi { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string AgentIdentity { get; set; } = string.Empty;
    
    public object ToLoggableProperties() => new
    {
        ConnectorHubApi,
        CustomerId,
        AgentIdentity
    };
}
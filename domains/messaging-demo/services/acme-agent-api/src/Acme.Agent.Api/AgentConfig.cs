namespace Acme.Agent.Api;

public class AgentConfig
{
    public string MessagingHubApi { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string AgentIdentity { get; set; } = string.Empty;
    
    public object ToLoggableProperties() => new
    {
        MessagingHubApi,
        CustomerId,
        AgentIdentity
    };
}
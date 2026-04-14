namespace Acme.Agent.Api;

public class AgentConfig
{
    public string SignalREndpoint { get; set; } = string.Empty;
    public string MessagingHubApi { get; set; } = string.Empty;
    
    public object ToLoggableProperties() => new
    {
        SignalREndpoint,
        MessagingHubApi
    };
}
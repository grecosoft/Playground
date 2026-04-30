namespace CSharp.Connector;

public class ConnectorConfig
{
    public string ConnectorHubApi { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string ConnectorId { get; set; } = string.Empty;
    
    public object ToLoggableProperties() => new
    {
        ConnectorHubApi,
        CustomerId,
        ConnectorId
    };
}
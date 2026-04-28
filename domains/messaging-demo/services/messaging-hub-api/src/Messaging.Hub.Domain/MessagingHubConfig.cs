namespace Messaging.Hub.Domain;

public class MessagingHubConfig
{ 
    public string SignalREndpoint { get; set; } = string.Empty;
    public string CommandSchemaRootPath { get; set; } = string.Empty;
    public object ToLoggableProperties() => new
    {
        SignalREndpoint
    };
}
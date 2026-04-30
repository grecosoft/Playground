using System.Text.Json.Serialization;

namespace CSharp.Connector.Commands;

public record ConnectorPingCommand(
    [property: JsonPropertyName("connectorId")] string ConnectorId,
    [property: JsonPropertyName("echoMessage")] string EchoMessage) ;
    
public record ConnectorPingResponse(
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("status")] string Status);
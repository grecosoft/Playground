
using System.Text.Json.Serialization;
using Common.Messaging.Entities;

namespace Messaging.Demo.Common.Commands;

[MessageNamespace("connector.commands.ping")]
public record ConnectorPingCommand(
    string ConnectorId,
    [property: JsonPropertyName("echoMessage")] string EchoMessage) : ConnectorCommand(ConnectorId), 
    ICommandMessage<ConnectorPingResponse>
{
    public ConnectorPingResponse? Response { get; set; }
}

public record ConnectorPingResponse(
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("status")] string Status);
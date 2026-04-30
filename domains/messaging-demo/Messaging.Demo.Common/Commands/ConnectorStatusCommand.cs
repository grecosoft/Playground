using System.Text.Json.Serialization;
using Common.Messaging.Entities;

namespace Messaging.Demo.Common.Commands;

[MessageNamespace("connector.commands.status")]
public record ConnectorStatusCommand(
    string ConnectorId,
    [property: JsonPropertyName("minLogLevel")] string MinLogLevel) : ConnectorCommand(ConnectorId), 
    ICommandMessage<ConnectorStatusResponse>
{
    public ConnectorStatusResponse? Response { get; set; }
}

public record ConnectorStatusResponse(
    [property: JsonPropertyName("generatedTimestamp")] DateTimeOffset GeneratedTimestamp,
    [property: JsonPropertyName("componentStatuses")] ComponentStatus[] ComponentStatuses);
    
public record ComponentStatus(
    [property: JsonPropertyName("componentId")] string ComponentId, 
    [property: JsonPropertyName("createdTimestamp")] DateTimeOffset CreatedTimestamp,
    [property: JsonPropertyName("lastLogMessage")] string LastLogMessage,
    [property: JsonPropertyName("logSeverity")] string LogSeverity);
    
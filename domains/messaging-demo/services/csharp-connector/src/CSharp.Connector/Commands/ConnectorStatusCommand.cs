using System.Text.Json.Serialization;

namespace CSharp.Connector.Commands;

public record ConnectorStatusCommand(
    [property: JsonPropertyName("connectorId")] string ConnectorId,
    [property: JsonPropertyName("minLogLevel")] string MinLogLevel);

public record ConnectorStatusResponse(
    [property: JsonPropertyName("generatedTimestamp")] DateTimeOffset GeneratedTimestamp,
    [property: JsonPropertyName("overallSeverity")] string OverallSeverity,
    [property: JsonPropertyName("componentStatuses")] ComponentStatus[] ComponentStatuses);
    
public record ComponentStatus(
    [property: JsonPropertyName("componentId")] string  ComponentId, 
    [property: JsonPropertyName("createdTimestamp")] DateTimeOffset CreatedTimestamp,
    [property: JsonPropertyName("lastLogMessage")] string LastLogMessage,
    [property: JsonPropertyName("logSeverity")] string LogSeverity);
    
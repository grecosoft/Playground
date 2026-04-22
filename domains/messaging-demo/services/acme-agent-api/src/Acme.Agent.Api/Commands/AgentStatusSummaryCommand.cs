using System.Text.Json.Serialization;

namespace Acme.Agent.Api.Commands;

public record AgentStatusSummaryCommand(
    [property: JsonPropertyName("companyId")] Guid CompanyId,
    [property: JsonPropertyName("AgentId")] string AgentId,
    [property: JsonPropertyName("MinLogLevel")] LogSeverityType MinLogLevel);

public record AgentStatusSummaryResponse(
    [property: JsonPropertyName("generatedTimestamp")] DateTimeOffset GeneratedTimestamp,
    [property: JsonPropertyName("overallSeverity")] LogSeverityType OverallSeverity,
    [property: JsonPropertyName("componentStatuses")] ComponentStatus[] ComponentStatuses);
    
public record ComponentStatus(
    [property: JsonPropertyName("componentId")] string  ComponentId, 
    [property: JsonPropertyName("createdTimestamp")] DateTimeOffset CreatedTimestamp,
    [property: JsonPropertyName("lastLogMessage")] string LastLogMessage,
    [property: JsonPropertyName("logSeverity")] LogSeverityType LogSeverity);
    
public enum LogSeverityType
{
    Debug = 1,
    Info,
    Warning,
    Error
}
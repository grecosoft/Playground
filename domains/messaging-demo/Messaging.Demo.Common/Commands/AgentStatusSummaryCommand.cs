using System.Text.Json.Serialization;
using Common.Messaging.Entities;

namespace Messaging.Demo.Common.Commands;

[MessageNamespace("agent.commands.status.summary")]
public record AgentStatusSummaryCommand(
    Guid CompanyId,
    string AgentId,
    [property: JsonPropertyName("minLogLevel")] LogSeverityType MinLogLevel) : AgentCommand(CompanyId, AgentId), 
    ICommandMessage<AgentStatusSummaryResponse>
{
    public AgentStatusSummaryResponse? Response { get; set; }
}

public record AgentStatusSummaryResponse(
    [property: JsonPropertyName("generatedTimestamp")] DateTimeOffset GeneratedTimestamp,
    [property: JsonPropertyName("overallSeverity")] LogSeverityType OverallSeverity,
    [property: JsonPropertyName("componentStatuses")] ComponentStatus[] ComponentStatuses);
    
public record ComponentStatus(
    [property: JsonPropertyName("componentId")] string ComponentId, 
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
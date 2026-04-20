namespace Acme.Agent.Api.Commands;

public record AgentStatusSummaryCommand(
    Guid CompanyId,
    string AgentId,
    LogSeverityType MinLogLeven);

public record AgentStatusSummaryResponse(
    DateTimeOffset GeneratedTimestamp,
    LogSeverityType OverallSeverity,
    ComponentStatus[] ComponentStatuses);
    
public record ComponentStatus(
    string  ComponentId, 
    DateTimeOffset CreatedTimestamp,
    string LastLogMessage,
    LogSeverityType LogSeverity);
    
public enum LogSeverityType
{
    Debug = 1,
    Info,
    Warning,
    Error
}
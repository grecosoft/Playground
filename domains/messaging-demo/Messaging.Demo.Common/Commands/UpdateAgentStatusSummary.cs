using Common.Messaging.Entities;

namespace Messaging.Demo.Common.Commands;

[MessageNamespace("agent.commands.status.summary")]
public record AgentStatusSummaryCommand(
    Guid CompanyId,
    string AgentId,
    LogSeverityType MinLogLeven) : AgentCommand(CompanyId, AgentId), 
    ICommandMessage<AgentStatusSummaryResponse>
{
    public AgentStatusSummaryResponse? Response { get; set; }
}

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
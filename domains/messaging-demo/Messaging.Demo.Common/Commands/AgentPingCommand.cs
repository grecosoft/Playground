
using Common.Messaging.Entities;

namespace Messaging.Demo.Common.Commands;

[MessageNamespace("agent.commands.ping")]
public record AgentPingCommand(
    Guid CompanyId,
    string AgentId,
    string EchoMessage) : AgentCommand(CompanyId, AgentId), 
    ICommandMessage<AgentPingResponse>
{
    public AgentPingResponse? Response { get; set; }
}

public record AgentPingResponse(string Message, string Status);
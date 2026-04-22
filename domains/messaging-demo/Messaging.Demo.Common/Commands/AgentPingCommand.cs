
using System.Text.Json.Serialization;
using Common.Messaging.Entities;

namespace Messaging.Demo.Common.Commands;

[MessageNamespace("agent.commands.ping")]
public record AgentPingCommand(
    Guid CompanyId,
    string AgentId,
    [property: JsonPropertyName("echoMessage")] string EchoMessage) : AgentCommand(CompanyId, AgentId), 
    ICommandMessage<AgentPingResponse>
{
    public AgentPingResponse? Response { get; set; }
}

public record AgentPingResponse(
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("status")] string Status);
using System.Text.Json.Serialization;

namespace Acme.Agent.Api.Commands;

public record AgentPingCommand(
    [property: JsonPropertyName("companyId")] Guid CompanyId,
    [property: JsonPropertyName("agentId")] string AgentId,
    [property: JsonPropertyName("echoMessage")] string EchoMessage) ;
    
public record AgentPingResponse(
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("status")] string Status);
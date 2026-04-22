using System.Text.Json.Serialization;

namespace Messaging.Demo.Common.Commands;

public record AgentCommand(
    [property: JsonPropertyName("companyId")] Guid CompanyId,
    [property: JsonPropertyName("agentId")] string AgentId);
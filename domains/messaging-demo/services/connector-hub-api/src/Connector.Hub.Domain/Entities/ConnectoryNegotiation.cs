namespace Connector.Hub.Domain.Entities;

public record AgentNegotiation(
    string Token = "",
    bool AgentNotFound = false,
    bool TokenNotGenerated = false);
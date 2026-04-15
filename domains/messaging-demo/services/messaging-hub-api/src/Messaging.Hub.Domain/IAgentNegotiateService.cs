using Messaging.Hub.Domain.Entities;

namespace Messaging.Hub.Domain;

public interface IAgentNegotiateService
{
    Task<AgentNegotiation> GetTokenAsync(
        Guid customerId,
        string agentIdentity,
        CancellationToken ct);
}
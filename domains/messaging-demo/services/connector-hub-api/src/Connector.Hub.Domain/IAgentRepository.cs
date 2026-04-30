using Connector.Hub.Domain.Entities;

namespace Connector.Hub.Domain;

public interface IAgentRepository
{
    Task<Agent?> ReadAgentAsync(Guid customerId, string agentIdentity, CancellationToken ct);
}
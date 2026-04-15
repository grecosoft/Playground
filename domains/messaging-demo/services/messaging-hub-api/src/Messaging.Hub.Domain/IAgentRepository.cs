using Messaging.Hub.Domain.Entities;

namespace Messaging.Hub.Domain;

public interface IAgentRepository
{
    Task<Agent?> ReadAgentAsync(Guid customerId, string agentIdentity, CancellationToken ct);
}
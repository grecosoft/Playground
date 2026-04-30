using Connector.Hub.Domain;
using Connector.Hub.Domain.Entities;

namespace Connector.Hub.Infra;

public class AgentRepository : IAgentRepository
{
    private readonly List<Agent> _agents =
    [
        new(
            id: Guid.Parse("025B3116-005C-4C17-A23D-583C55A5CF30"),
            customerId: Guid.Parse("E6B2BFA1-851B-4DCC-B4A3-CBCAF8FFE138"),
            agentIdentity: "agent1"),


        new(
            id: Guid.Parse("36AB6C21-FDAA-45CC-8A9D-5B650B7CF1DC"),
            customerId: Guid.Parse("E6B2BFA1-851B-4DCC-B4A3-CBCAF8FFE138"),
            agentIdentity: "agent2"),


        new(
            id: Guid.Parse("11B6C950-AFA0-4D8B-A44A-7982180F58D8"),
            customerId: Guid.Parse("68CE91EB-9E4B-474F-BB13-50244184393E"),
            agentIdentity: "agent3")
    ];
    
    public Task<Agent?> ReadAgentAsync(Guid customerId, string identity, CancellationToken ct)
    {
       var agent = _agents.FirstOrDefault(a => a.CustomerId == customerId && a.AgentIdentity == identity);
       return Task.FromResult(agent);
    }
}
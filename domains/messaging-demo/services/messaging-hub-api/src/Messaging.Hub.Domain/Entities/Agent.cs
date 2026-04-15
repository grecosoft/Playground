namespace Messaging.Hub.Domain.Entities;

public class Agent(Guid id, Guid customerId, string agentIdentity)
{
    public Guid Id { get; init; } = id;
    public Guid CustomerId { get; init; } = customerId;
    public string AgentIdentity { get; init; } = agentIdentity;
}
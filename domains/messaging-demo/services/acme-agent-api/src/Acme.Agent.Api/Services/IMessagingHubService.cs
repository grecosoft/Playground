namespace Acme.Agent.Api.Services;

public interface IMessagingHubService
{
    Task<string> GetAgentTokenAsync(Guid customerId, string identity, CancellationToken ct);
}
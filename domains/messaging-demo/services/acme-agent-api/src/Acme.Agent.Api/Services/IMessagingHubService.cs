namespace Acme.Agent.Api.Services;

public interface IMessagingHubService
{
    Task<string> GetHubTokenAsync(string agentId, CancellationToken ct);
}
namespace Acme.Agent.Api.Services;

public class MessagingHubService(
    HttpClient httpClient) : IMessagingHubService
{
    public async Task<string> GetHubTokenAsync(string agentId, CancellationToken ct)
    {
        var response = await httpClient.GetFromJsonAsync<string>(
            $"hub/{agentId}/token", ct);
            
        return response ?? throw new InvalidOperationException("Agent Hub Token not returned");
    }
}
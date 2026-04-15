namespace Acme.Agent.Api.Services;

public class MessagingHubService(
    HttpClient httpClient) : IMessagingHubService
{
    public async Task<string> GetAgentTokenAsync(Guid customerId, string identity, CancellationToken ct)
    {
        var response = await httpClient.GetFromJsonAsync<TokenResponse>(
            $"{customerId}/hub/{identity}/token", ct);
            
        return response?.Token ?? throw new InvalidOperationException("Agent Hub Token not returned");
    }
    
    private record TokenResponse(string Token);
}
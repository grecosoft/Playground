using System.Security.Claims;
using Messaging.Hub.Domain;
using Messaging.Hub.Domain.Entities;
using Microsoft.Azure.SignalR.Management;

namespace Messaging.Hub.Api.Services;

public class AgentNegotiateService(
    ILogger<AgentNegotiateService> logger,
    ServiceManager manager,
    IAgentRepository agentRepository) : IAgentNegotiateService
{
    public async Task<AgentNegotiation> GetTokenAsync(
        Guid customerId,
        string agentIdentity,
        CancellationToken ct)
    {
        var agent = await agentRepository.ReadAgentAsync(customerId, agentIdentity, ct);
        if (agent is null)
        {
            return new AgentNegotiation(AgentNotFound: true);
        }
        
        var hubContext = await manager.CreateHubContextAsync("ConnectorHub/negotiate", ct);
        var options = new NegotiationOptions
        {
            UserId = agent.AgentIdentity,
            Claims = [
                new Claim("agent-id", agent.Id.ToString()),
                new Claim("agent-identity", agent.AgentIdentity),
                new Claim("agent-company", agent.CustomerId.ToString())
            ]
        };
        
        var negotiationResponse = await hubContext.NegotiateAsync(options, ct);
        if (string.IsNullOrEmpty(negotiationResponse.Error))
        {
            return new AgentNegotiation(Token: negotiationResponse.AccessToken!);
        }

        logger.LogError(
            "Negotiation returned an error: {error} for customerId: {customerId} agent: {agentIdentity}",
            negotiationResponse.Error,
            customerId,
            agentIdentity);
            
        return new AgentNegotiation(TokenNotGenerated: true);
    }
}
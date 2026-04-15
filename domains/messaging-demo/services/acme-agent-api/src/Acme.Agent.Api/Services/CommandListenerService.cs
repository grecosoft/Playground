using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

namespace Acme.Agent.Api.Services;

public class CommandListenerService(
    ILogger<CommandListenerService> logger,
    IOptions<AgentConfig> agentConfigOptions,
    IMessagingHubService messagingHubService) : BackgroundService
{
    private readonly AgentConfig _agentConfig = agentConfigOptions.Value;
    private HubConnection? _connection;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!Guid.TryParse(_agentConfig.CustomerId, out var customerId))
        {
            logger.LogError("Invalid customer id: {customerId}", _agentConfig.CustomerId);
            return;
        }
        
        _connection = new HubConnectionBuilder()
            .WithUrl($"{_agentConfig.MessagingHubApi}/ConnectorHub", options =>
            {
                options.AccessTokenProvider = () => messagingHubService.GetAgentTokenAsync(
                    customerId,
                    _agentConfig.AgentIdentity, stoppingToken)!;
            })
            .WithAutomaticReconnect([
                TimeSpan.FromSeconds(0),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(30)
            ])
            .Build();

        _connection.On<string>("command-message", command =>
        {
            logger.LogInformation("Received command: {command}", command);
            // handle command...
        });

        await _connection.StartAsync(stoppingToken);

        // Keep alive until cancelled
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
    
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_connection is not null)
            await _connection.DisposeAsync();

        await base.StopAsync(cancellationToken);
    }
}
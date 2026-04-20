using Acme.Agent.Api.Commands;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

namespace Acme.Agent.Api.Services;

public class CommandListenerService(
    ILogger<CommandListenerService> logger,
    IOptions<AgentConfig> agentConfigOptions) : BackgroundService
{
    private readonly AgentConfig _agentConfig = agentConfigOptions.Value;
    private HubConnection? _connection;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl($"{_agentConfig.MessagingHubApi}/connectorhub?agentIdentity={_agentConfig.AgentIdentity}")
            .WithAutomaticReconnect([
                TimeSpan.FromSeconds(0),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(30)
            ])
            .Build();
        
        HandleAgentPingCommand(_connection);

        await _connection.StartAsync(stoppingToken);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private void HandleAgentPingCommand(HubConnection connection)
    {
        connection.On<AgentPingCommand>("agent.commands.ping", command =>
        {
            logger.LogInformation("Received Ping Command: {command}", command);
            // handle command...
        });
    }
    
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_connection is not null)
            await _connection.DisposeAsync();

        await base.StopAsync(cancellationToken);
    }
}
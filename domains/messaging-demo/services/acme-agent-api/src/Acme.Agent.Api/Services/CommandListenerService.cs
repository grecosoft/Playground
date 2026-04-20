using Acme.Agent.Api.Commands;
using Microsoft.AspNetCore.SignalR.Client;

namespace Acme.Agent.Api.Services;

public class CommandListenerService(
    ILogger<CommandListenerService> logger,
    HubConnection hubConnection) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        HandleAgentPingCommand(hubConnection);
        HandleAgentStatusSummaryCommand(hubConnection);

        await hubConnection.StartAsync(stoppingToken);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private void HandleAgentPingCommand(HubConnection connection)
    {
        connection.On<AgentPingCommand, AgentPingResponse>("agent.commands.ping", command =>
        {
            logger.LogInformation("Received Ping Command: {command}", command);
            
            // handle command...
            
            return new AgentPingResponse($"Echo: {command.EchoMessage}", Guid.NewGuid().ToString());
        });
    }
    
    private void HandleAgentStatusSummaryCommand(HubConnection connection)
    {
        connection.On<string, AgentStatusSummaryCommand>("agent.commands.status.summary", (correlationId, command) =>
        {
            logger.LogInformation("Received Status Summary Command: {command}", command);
            
            // Queue command for processing....
            // This will be work done by the agent and reported back when complete, so no response is returned here.
        });
    }
    
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await hubConnection.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}
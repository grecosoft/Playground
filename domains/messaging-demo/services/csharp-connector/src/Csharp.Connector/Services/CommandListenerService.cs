using System.Text.Json;
using CSharp.Connector.Commands;
using CSharp.Connector.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace CSharp.Connector.Services;

public class CommandListenerService(
    ILogger<CommandListenerService> logger,
    HubConnection hubConnection) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        HandleAgentPingCommand(hubConnection);
        HandleAgentStatusSummaryCommand(hubConnection);
        HandleValidationErrors(hubConnection);

        await hubConnection.StartAsync(stoppingToken);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private void HandleValidationErrors(HubConnection connection)
    {
        connection.On<string, CommandReplyResultModel>("command.reply.validation.error", (correlationId, result) =>
        {
            logger.LogWarning(
                "Received validation error for correlation {id}: {message}",
                correlationId, 
                result.Message);
        });
    }

    private void HandleAgentPingCommand(HubConnection connection)
    {
        connection.On<string, AgentPingCommand, string>("agent.commands.ping", (correlationId, command) =>
        {
            logger.LogInformation(
                "Received Ping Command: {@command} for correlation {id}",
                command, 
                correlationId);
            
            // handle command...
            
            return JsonSerializer.Serialize(new AgentPingResponse(
                $"Echo: {command.EchoMessage}", 
                Guid.NewGuid().ToString()));
        });
    }
    
    private void HandleAgentStatusSummaryCommand(HubConnection connection)
    {
        connection.On<string, AgentStatusSummaryCommand>("agent.commands.status.summary", (correlationId, command) =>
        {
            logger.LogInformation(
                "Received Status Summary Command: {@command} for correlation {id}",
                command, 
                correlationId);
            
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
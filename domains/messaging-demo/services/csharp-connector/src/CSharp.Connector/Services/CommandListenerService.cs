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
        HandlePingCommand(hubConnection);
        HandleStatusCommand(hubConnection);
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

    private void HandlePingCommand(HubConnection connection)
    {
        connection.On<string, ConnectorPingCommand, string>("connector.commands.ping", (correlationId, command) =>
        {
            logger.LogInformation(
                "Received Ping Command: {@command} for correlation {id}",
                command, 
                correlationId);
            
            // handle command...
            
            return JsonSerializer.Serialize(new ConnectorPingResponse(
                $"Echo: {command.EchoMessage}", 
                Guid.NewGuid().ToString()));
        });
    }
    
    private void HandleStatusCommand(HubConnection connection)
    {
        connection.On<string, ConnectorStatusCommand>("connector.commands.status", (correlationId, command) =>
        {
            logger.LogInformation(
                "Received Status Summary Command: {@command} for correlation {id}",
                command, 
                correlationId);
            
            // Queue command for processing....
            // This will be work done by connector and reported back when complete, so no response is returned here.
        });
    }
    
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await hubConnection.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}
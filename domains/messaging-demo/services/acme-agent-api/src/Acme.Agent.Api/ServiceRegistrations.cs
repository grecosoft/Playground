using Acme.Agent.Api.Services;
using Microsoft.AspNetCore.SignalR.Client;

namespace Acme.Agent.Api;

public static class ServiceRegistrations
{
    public static void AddServices(
        this IHostApplicationBuilder builder,
        ILogger bootstrapLogger)
    {
        var config = builder.Configuration.Get<AgentConfig>()
                     ?? throw new NullReferenceException("AgentConfig is null");

        bootstrapLogger.LogInformation(
            "Configuring Service Messaging: {@Configuration}", config.ToLoggableProperties());
        
        builder.Services.Configure<AgentConfig>(builder.Configuration);
        builder.Services.AddHostedService<CommandListenerService>();
        
        var connection = new HubConnectionBuilder()
            .WithUrl($"{config.MessagingHubApi}/connectorhub?agentIdentity={config.AgentIdentity}")
            .WithAutomaticReconnect([
                TimeSpan.FromSeconds(0),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(30)
            ])
            .Build();
        
        builder.Services.AddSingleton(connection);
    }
}
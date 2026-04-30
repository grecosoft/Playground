using CSharp.Connector.Services;
using Microsoft.AspNetCore.SignalR.Client;

namespace CSharp.Connector;

public static class ServiceRegistrations
{
    public static void AddServices(
        this IHostApplicationBuilder builder,
        ILogger bootstrapLogger)
    {
        var config = builder.Configuration.Get<ConnectorConfig>()
                     ?? throw new NullReferenceException("ConnectorConfig is null");

        bootstrapLogger.LogInformation(
            "Configuring Service Messaging: {@Configuration}", config.ToLoggableProperties());
        
        builder.Services.Configure<ConnectorConfig>(builder.Configuration);
        builder.Services.AddHostedService<CommandListenerService>();
        
        var connection = new HubConnectionBuilder()
            .WithUrl($"{config.ConnectorHubApi}/connectorhub?connectorid={config.ConnectorId}")
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
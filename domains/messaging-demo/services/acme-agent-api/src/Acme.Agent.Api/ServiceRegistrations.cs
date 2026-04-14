using Acme.Agent.Api.Services;

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
        
        builder.Services.AddHttpClient<IMessagingHubService, MessagingHubService>(client =>
        {
            client.BaseAddress = new Uri($"{config.MessagingHubApi}/"); 
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        builder.Services.AddHostedService<CommandListenerService>();
    }
}
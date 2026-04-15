using Azure.Identity;
using Messaging.Hub.Domain;
using Messaging.Hub.Infra;
using Microsoft.Azure.SignalR;
using Microsoft.Azure.SignalR.Management;

namespace Messaging.Hub.Api;

public static class ServiceRegistrations
{
  
    public static IHostApplicationBuilder AddSignalRMessaging(
        this IHostApplicationBuilder builder,
        ILogger bootstrapLogger)
    {
        var config = builder.Configuration.Get<MessagingHubConfig>() 
                     ?? throw new NullReferenceException("MessagingHubConfig is null");
        
        bootstrapLogger.LogInformation(
            "Configuring Service Messaging: {@Configuration}", config.ToLoggableProperties());
        
        var credential = new DefaultAzureCredential();
        
        builder.Services.AddSignalR().AddAzureSignalR(options =>
        {
            options.Endpoints =
            [
                new ServiceEndpoint(new Uri(config.SignalREndpoint), credential)
            ];
        });

        var serviceManager = new ServiceManagerBuilder()
            .WithOptions(options =>
            {
                options.ServiceEndpoints =
                [
                    new ServiceEndpoint(new Uri(config.SignalREndpoint), credential)
                ];
                    
            })
            .BuildServiceManager();
        
        builder.Services.AddSingleton(serviceManager);
        return builder;
    }
    
    public static IApplicationBuilder MapConnectorHub(this IApplicationBuilder appBuilder)
    {
        appBuilder.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<ConnectorHub>("/ConnectorHub");
        });
        
        return appBuilder;
    }
}

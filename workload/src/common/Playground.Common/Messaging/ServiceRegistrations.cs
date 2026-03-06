using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Playground.Common.Messaging.Services;

namespace Playground.Common.Messaging;

public static class ServiceRegistrations
{
    public static IServiceCollection AddBusMessaging(this IServiceCollection services,
        IConfiguration configuration)
    {
        var configSection = configuration.GetSection("BusMessaging");
        
        var config = configSection.Get<BusMessagingConfig>() 
            ?? throw new NullReferenceException("BusMessagingConfig is null");

        services.AddHostedService<RequestMessageDispatcher>();
        
        AddMessagingBus(services, config);
        AddRequestQueueProcessor(services, config);
        AddReplyQueueProcessor(services, config);
        AddCommandMessageHandlers(services);
        
        AddDependentServices(services, config);
        return services;
    }

    private static void AddMessagingBus(IServiceCollection services, BusMessagingConfig config)
    {
        services.AddSingleton(new ServiceBusClient(config.FullyQualifiedNamespace, new DefaultAzureCredential()));
    }
    
    private static void AddDependentServices(IServiceCollection services, BusMessagingConfig config)
    {
        foreach (var (serviceKey, serviceConfig) in config.DependentServices)
        {
            if (!string.IsNullOrEmpty(serviceConfig.RequestQueue))
            {
                services.AddKeyedSingleton<IMessagingService>(serviceKey, (sp, _) =>
                {
                    var client = sp.GetRequiredService<ServiceBusClient>();
                    var sender = client.CreateSender(serviceConfig.RequestQueue);
                    return new MessagingService(config, client, sender);
                });
            }

            if (!string.IsNullOrEmpty(serviceConfig.ReplyQueue))
            {
                services.AddKeyedSingleton(serviceConfig.ReplyQueue, (sp, _) =>
                {
                    var client = sp.GetRequiredService<ServiceBusClient>();
                    return client.CreateSender(serviceConfig.ReplyQueue);
                });
            }
            
        }
    }
    
    private static void AddRequestQueueProcessor(IServiceCollection services, BusMessagingConfig config)
    {
        services.AddKeyedSingleton("datalab.messaging.request", (sp, _) =>
        {
            var client = sp.GetRequiredService<ServiceBusClient>();
            return client.CreateProcessor(config.RequestQueue, new ServiceBusProcessorOptions
            {
                ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete,
                PrefetchCount = 100
            });
        });
    }

    private static void AddReplyQueueProcessor(IServiceCollection services, BusMessagingConfig config)
    {
        services.AddKeyedSingleton("datalab.messaging.reply", (sp, _) =>
        {
            var client = sp.GetRequiredService<ServiceBusClient>();
            return client.CreateProcessor(config.ReplyQueue);
        });
    }

    private static void AddCommandMessageHandlers(IServiceCollection services)
    {
        var commandHandlers = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.DefinedTypes)
            .GetCommandDispatches();

        foreach (var handler in commandHandlers)
        {
            services.AddKeyedScoped(typeof(ICommandMessageHandler), handler.CommandNamespace, handler.ImplementationType);
        }
    }
    
}
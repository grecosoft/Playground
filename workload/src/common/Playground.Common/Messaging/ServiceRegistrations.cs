using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Playground.Common.Messaging.Services;

namespace Playground.Common.Messaging;

public static class ServiceRegistrations
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddBusMessaging(IConfiguration configuration, string solutionName)
        {

            var configSection = configuration.GetSection("BusMessaging");
        
            var config = configSection.Get<BusMessagingConfig>() 
                         ?? throw new NullReferenceException("BusMessagingConfig is null");

            AddMessagingBus(services, config);
            services.AddHostedService<CommandHandlerDispatcher>();
            
            
            AddCommandMessageHandlers(services);
        

            AddRequestQueueProcessor(services, config);
            AddReplyToQueueSender(services, config);
            AddReplyQueueProcessor(services, config);
            
            // AddReplyQueueProcessor(services, config);
            

            services.AddSingleton<IMessagingService>(sp =>
            {
                var client = sp.GetRequiredService<ServiceBusClient>();
                var sender = client.CreateSender(config.CommandTopic);
                return new MessagingService(solutionName, config, client, sender);
            });

            return services;
        }
    }

    private static void AddMessagingBus(IServiceCollection services, BusMessagingConfig config)
    {
        services.AddSingleton(new ServiceBusClient(config.FullyQualifiedNamespace, new DefaultAzureCredential()));
    }
    
    
    
    private static void AddRequestQueueProcessor(IServiceCollection services, BusMessagingConfig config)
    {
        services.AddKeyedSingleton("datalab.messaging.request", (sp, _) =>
        {
            var client = sp.GetRequiredService<ServiceBusClient>();
            return client.CreateProcessor(config.CommandTopic, "solution-service-one-commands", new ServiceBusProcessorOptions
            {
                ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete,
                PrefetchCount = 100
            });
        });
    }

    private static void AddReplyToQueueSender(IServiceCollection services, BusMessagingConfig config)
    {
        services.AddKeyedSingleton("datalab.messaging.reply.queue", (sp, _) =>
        {
            var client = sp.GetRequiredService<ServiceBusClient>();
            var sender = client.CreateSender(config.CommandTopic);
            return sender;
        });
    }

    private static void AddReplyQueueProcessor(IServiceCollection services, BusMessagingConfig config)
    {
        services.AddKeyedSingleton("datalab.messaging.reply", (sp, _) =>
        {
            var client = sp.GetRequiredService<ServiceBusClient>();
            return client.CreateSender(config.ReplyQueue);
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
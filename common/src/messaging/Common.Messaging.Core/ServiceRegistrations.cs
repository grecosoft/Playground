using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Extensions.Logging;

namespace Common.Messaging.Core;

public static class ServiceRegistrations
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddBusMessaging(IConfiguration configuration)
        {
            var configSection = configuration.GetSection("BusMessaging");
        
            var config = configSection.Get<BusMessagingConfig>() 
                         ?? throw new NullReferenceException("BusMessagingConfig is null");

            AddMessagingBus(services, config);
            
            AddCommandMessageHandlers(services);
            AddRequestQueueProcessor(services, config);
            AddReplyQueueSender(services, config);
            AddDependentServiceEndpoints(services, config);
            
            services.AddHostedService<RpcCommandHandlerDispatcher>();
            services.AddSingleton<ICommandRepository, CommandRepository>();
            
            services.AddSingleton<MessagingService>(sp =>
            {
                var client = sp.GetRequiredService<ServiceBusClient>();
                var sender = client.CreateSender(config.SolutionCommandTopic);
                return new MessagingService(config, client, sender);
            });
            
            services.AddSingleton<IMessagingService>(sp => sp.GetRequiredService<MessagingService>());
            
            return services;
        }
    }

    private static void AddMessagingBus(IServiceCollection services, BusMessagingConfig config)
    {
        services.AddSingleton(new ServiceBusClient(
            config.FullyQualifiedNamespace,
            new DefaultAzureCredential(),
            new ServiceBusClientOptions
            {
                
            }));
    }
    
    private static void AddCommandMessageHandlers(IServiceCollection services)
    {
        var commandHandlers = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.DefinedTypes)
            .GetCommandDispatches();

        foreach (var handler in commandHandlers)
        {
            services.AddKeyedScoped(
                typeof(ICommandMessageHandler),
                handler.CommandNamespace,
                handler.ImplementationType);
        }
    }
    
    
    private static void AddRequestQueueProcessor(IServiceCollection services, BusMessagingConfig config)
    {
        services.AddKeyedSingleton("datalab.messaging.rpc.request", (sp, _) =>
        {
            var client = sp.GetRequiredService<ServiceBusClient>();
            return client.CreateProcessor(
                config.SolutionCommandTopic,
                config.ServiceCommandSubscription, 
                new ServiceBusProcessorOptions
                {
                    ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete,
                    PrefetchCount = 100
                });
        });
    }
    
    private static void AddReplyQueueSender(IServiceCollection services, BusMessagingConfig config)
    {
        services.AddKeyedSingleton("datalab.messaging.rpc.reply", (sp, _) =>
        {
            var client = sp.GetRequiredService<ServiceBusClient>();
            return client.CreateSender(config.SolutionReplyQueue);
        });
    }
    
    private static void AddDependentServiceEndpoints(IServiceCollection serviceCollection, BusMessagingConfig config)
    {
        foreach (var (serviceKey, serviceInfo) in config.DependentServices)
        {
            serviceCollection.AddKeyedSingleton<IServiceEndpoint>(serviceKey, (sp, _)  =>
            {
                var messagingService = sp.GetRequiredService<MessagingService>();

                var logger = new SerilogLoggerFactory(Log
                        .ForContext("DependentServiceName", serviceInfo.Name)
                        .ForContext("DependentServiceId", serviceInfo.Id) )
                 
                        .CreateLogger(config.ServiceName);
                
                return new ServiceEndpoint(
                    logger, 
                    new EndpointInfo(serviceInfo.Name, serviceInfo.Id),
                    messagingService);
            });
        }
    }
}
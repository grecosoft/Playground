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

            AddServiceBusClient(services, config);
            AddRpcCommandMessageHandling(services, config);
            // AddAsyncCommandMessageHandling(services, config);
            
            AddCommandMessageHandlers(services);
            
            // Registers services used for sending messages to other services, which will
            // be injected into command handlers and other services that need to send messages.
            AddMessagingServices(services, config);
            AddDependentServiceEndpoints(services, config);
            return services;
        }
    }

    private static void AddServiceBusClient(IServiceCollection services, BusMessagingConfig config)
    {
        services.AddSingleton(new ServiceBusClient(
            config.ServiceBusNamespace,
            new DefaultAzureCredential(),
            new ServiceBusClientOptions
            {
                
            }));
    }
    
    private static void AddRpcCommandMessageHandling(
        IServiceCollection services,
        BusMessagingConfig config)
    {
        // Subscribes to the topic/subscription to receive RPC commands, which will be dispatched
        // to the appropriate handlers based on message properties.
        services.AddKeyedSingleton("rpc", (sp, _) =>
        {
            var client = sp.GetRequiredService<ServiceBusClient>();
            return client.CreateProcessor(
                config.RpcCommandTopic,
                config.RpcCommandSubscription, 
                new ServiceBusProcessorOptions
                {
                    ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete,
                });
        });
        
        // Registers a sender for sending replies to RPC commands back to the originating service.
        services.AddKeyedSingleton("rpc", (sp, _) =>
        {
            var client = sp.GetRequiredService<ServiceBusClient>();
            return client.CreateSender(config.RpcReplyQueue);
        });
        
        services.AddHostedService<CommandHandlerDispatcherRpc>();
    }

    private static void AddAsyncCommandMessageHandling(
        IServiceCollection services,
        BusMessagingConfig config)
    {
        services.AddKeyedSingleton("async", (sp, _) =>
        {
            var client = sp.GetRequiredService<ServiceBusClient>();
            return client.CreateProcessor(
                config.AsyncCommandTopic,
                config.AsyncCommandSubscription, 
                new ServiceBusProcessorOptions
                {
              
                });
        });
    }
    
    private static void AddMessagingServices(
        IServiceCollection services,
        BusMessagingConfig config)
    {
        services.AddSingleton<MessagingService>(sp =>
        {
            var client = sp.GetRequiredService<ServiceBusClient>();
            var sender = client.CreateSender(config.RpcCommandTopic);
            return new MessagingService(config, client, sender);
        });
            
        services.AddSingleton<IMessagingService>(sp => sp.GetRequiredService<MessagingService>());
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
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Common.Messaging.Commands;
using Common.Messaging.Core.Commands;
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
        
            var config = configSection.Get<MessagingConfig>() 
                         ?? throw new NullReferenceException("BusMessagingConfig is null");
            
            services.Configure<MessagingConfig>(configSection);

            AddServiceBusClient(services, config);
            AddRpcCommandMessageHandling(services, config);
            AddAsyncCommandMessageHandling(services, config);
            
            AddCommandMessageHandlers(services);
            
            // Registers services used for sending messages to other services, which will
            // be injected into command handlers and other services that need to send messages.
            AddMessagingServices(services, config);
            AddDependentServiceEndpoints(services, config);
            return services;
        }
    }

    private static void AddServiceBusClient(IServiceCollection services, MessagingConfig config)
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
        MessagingConfig config)
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
        services.AddKeyedSingleton("rpc-reply", (sp, _) =>
        {
            var client = sp.GetRequiredService<ServiceBusClient>();
            return client.CreateSender(config.RpcReplyQueue);
        });
        
        services.AddHostedService<CommandHandlerDispatcherRpc>();
    }

    private static void AddAsyncCommandMessageHandling(
        IServiceCollection services,
        MessagingConfig config)
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
        
        services.AddHostedService<CommandHandlerDispatcherAsync>();
    }
    
    private static void AddMessagingServices(
        IServiceCollection services,
        MessagingConfig config)
    {
        services.AddKeyedSingleton("rpc", (sp, _) =>
        {
            var client = sp.GetRequiredService<ServiceBusClient>();
            return client.CreateSender(config.RpcCommandTopic);
        });
        
        services.AddKeyedSingleton("async", (sp, _) =>
        {
            var client = sp.GetRequiredService<ServiceBusClient>();
            return client.CreateSender(config.AsyncCommandTopic);
        });
        
        services.AddSingleton<CommandMessagingService>();
        services.AddSingleton<ICommandMessagingService>(sp => sp.GetRequiredService<CommandMessagingService>());
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
    
    private static void AddDependentServiceEndpoints(IServiceCollection serviceCollection, MessagingConfig config)
    {
        foreach (var (serviceKey, serviceInfo) in config.DependentServices)
        {
            serviceCollection.AddKeyedSingleton<ICommandEndpoint>(serviceKey, (sp, _)  =>
            {
                var messagingService = sp.GetRequiredService<CommandMessagingService>();

                var logger = new SerilogLoggerFactory(Log
                        .ForContext("DependentServiceName", serviceInfo.Name)
                        .ForContext("DependentServiceId", serviceInfo.Id) )
                 
                    .CreateLogger(config.ServiceName);
                
                return new CommandEndpoint(
                    logger, 
                    new EndpointInfo(serviceInfo.Name, serviceInfo.Id),
                    messagingService);
            });
        }
    }
}
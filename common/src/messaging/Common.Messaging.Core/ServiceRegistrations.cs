using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Common.Messaging.Commands;
using Common.Messaging.Core.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Common.Messaging.Core;

[SuppressMessage("Performance", "CA1873:Avoid potentially expensive logging")]
public static class ServiceRegistrations
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddBusMessaging(
            ILogger bootstrapLogger,
            IConfiguration configuration)
        {
            // Load and log the messaging configuration:
            var configSection = configuration.GetSection("ServiceMessaging");
        
            var config = configSection.Get<MessagingConfig>() 
                         ?? throw new NullReferenceException("BusMessagingConfig is null");
            
            bootstrapLogger.LogInformation(
                "Configuring Service Messaging: {Configuration}",
                JsonSerializer.Serialize(config.ToLoggableProperties()));
            
            services.Configure<MessagingConfig>(configSection);
            
            AddServiceBusClient(services, config);
            AddRpcCommandMessageHandling(services, config);
            AddAsyncCommandMessageHandling(services, config);
            
            AddCommandMessageHandlers(services);
            AddMessagingServices(services, config);
            return services;
        }
    }

    private static void AddServiceBusClient(IServiceCollection services, MessagingConfig config)
    {
        services.AddSingleton(new ServiceBusClient(
            config.ServiceBusHostName,
            new DefaultAzureCredential(),
            new ServiceBusClientOptions
            {
                Identifier = config.ServiceName
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
                    Identifier = $"{config.ServiceName}-rpc-command-processor",
                    ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete,
                    PrefetchCount = config.RpcProcesser.PrefetchCount,
                    MaxConcurrentCalls = config.RpcProcesser.MaxConcurrentCalls,
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
                    Identifier = $"{config.ServiceName}-async-command-processor",
                    PrefetchCount = config.AsyncProcesser.PrefetchCount,
                    MaxConcurrentCalls = config.AsyncProcesser.MaxConcurrentCalls,
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
        
        services.AddSingleton<CommandMessaging>();
        services.AddSingleton<ICommandMessaging>(sp => sp.GetRequiredService<CommandMessaging>());
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
}
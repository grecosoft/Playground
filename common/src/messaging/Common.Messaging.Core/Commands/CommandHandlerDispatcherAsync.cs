using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Common.Messaging.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Common.Messaging.Core.Commands;

/// <summary>
/// Subscribes to the Service Bus topic/subscription on which asynchronous commands are delevered between services for
/// commands targeting this service, and dispatches received commands to the appropriate command handler for processing.
/// </summary>
/// <param name="messagingOptions">Messaging related configurations.</param>
/// <param name="logger">Logger.</param>
/// <param name="serviceProvider">Service provider used to create scope to execute handler within.</param>
/// <param name="requestTopicProcessor">The processor on which the commands are received.</param>
public class CommandHandlerDispatcherAsync(
    IOptions<MessagingConfig> messagingOptions,
    ILogger<CommandHandlerDispatcherAsync> logger,
    IServiceProvider serviceProvider,
    [FromKeyedServices("async")]ServiceBusProcessor requestTopicProcessor) : BackgroundService
{
    private readonly MessagingConfig _msgConfig = messagingOptions.Value;
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        requestTopicProcessor.ProcessMessageAsync += OnProcessMessageAsync;
        requestTopicProcessor.ProcessErrorAsync += OnProcessErrorAsync;
        
        return requestTopicProcessor.StartProcessingAsync(stoppingToken);
    }

    private async Task OnProcessMessageAsync(ProcessMessageEventArgs eventArgs)
    {
        // Create service scope to execute the request within:
        using var requestScope = serviceProvider.CreateScope();
        try
        {
            var context = CommandContext.Create(eventArgs.Message);
            using var _ = logger.BeginScope(context.ToDictionary());
            
            logger.LogDebug(
                "Received Response[{Destination}<={Source}]({Namespace}:{CorrelationId})", 
                _msgConfig.ServiceName,
                context.SendingServiceName,
                context.CommandNamespace,
                context.CorrelationId);
            
            var payload = JsonSerializer.Deserialize<CommandPayload>(eventArgs.Message.Body);
            if (payload is null)
            {
                logger.LogError(
                    "The received message could not be deserialized into type: {PayloadType}.",
                    nameof(CommandPayload));
                return;
            }
            
            // Resolve the command handler and set the command on the context:
            var commandHandler = requestScope.ServiceProvider.GetCommandHandler(context);
            context.SetCommand(payload.Command, commandHandler.CommandType);
            
            // Determine if the received command is a response to a previously sent command.
            if (payload.HasResponse)
            {
                context.SetResponse(payload.Response, commandHandler.ResponseType!);
            }

            // Resolve the repository.  If this code is being invoked by the receiving service
            // the repository is used to save the command until its response can be determined.
            var commandRepository = requestScope.ServiceProvider.GetService<ICommandRepository>();
            if (commandRepository is not null)
            {
                context.SetCommandRepository(commandRepository);
            }
            
            logger.LogDebug(
                "Dispatching command message {MessageId} to handler {Handler} of type {CommandType}.",
                eventArgs.Message.MessageId, 
                commandHandler,
                commandHandler.CommandType);
            
            // Send the command to the handler for processing.
            await commandHandler.Handle(context, eventArgs.CancellationToken);
            await eventArgs.CompleteMessageAsync(eventArgs.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception processing message {MessageId}.", eventArgs.Message.MessageId);
        }
    }
    
    private Task OnProcessErrorAsync(ProcessErrorEventArgs eventArgs)
    {
        logger.LogError(
            eventArgs.Exception, 
            "Exception processing message received on {QueueName}",
            eventArgs.EntityPath);
        
        return Task.CompletedTask;
    }
    
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping processing of Async commands.");
        return base.StopAsync(cancellationToken);
    }
}
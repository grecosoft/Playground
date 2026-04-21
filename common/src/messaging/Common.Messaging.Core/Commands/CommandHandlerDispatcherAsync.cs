using Azure.Messaging.ServiceBus;
using Common.Messaging.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Common.Messaging.Core.Commands;

/// <summary>
/// Subscribes to the Service Bus topic/subscription on which asynchronous commands are delevered between services for
/// commands targeting this service, and dispatches received commands to the appropriate command handler for processing.
/// </summary>
/// <param name="logger">Logger.</param>
/// <param name="serviceProvider">Service provider used to create scope to execute handler within.</param>
/// <param name="requestTopicProcessor">The processor on which the commands are received.</param>
public class CommandHandlerDispatcherAsync(
    ILogger<CommandHandlerDispatcherAsync> logger,
    IServiceProvider serviceProvider,
    [FromKeyedServices("async")]ServiceBusProcessor requestTopicProcessor) : BackgroundService
{
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
            
            var commandHandler = requestScope.ServiceProvider.GetCommandHandler(context);
            context.SetCommand(eventArgs.Message.Body, commandHandler.CommandType);

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
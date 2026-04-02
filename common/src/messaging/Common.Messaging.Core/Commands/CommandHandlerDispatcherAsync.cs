using Azure.Messaging.ServiceBus;
using Common.Messaging.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Common.Messaging.Core.Commands;

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
            var commandRepository = requestScope.ServiceProvider.GetRequiredService<ICommandRepository>();
            var commandHandler = requestScope.ServiceProvider.GetCommandHandler(context);
            
            context.SetCommand(eventArgs.Message.Body, commandHandler.CommandType);
            context.SetCommandRepository(commandRepository);
            
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
        logger.LogError(eventArgs.Exception, "Exception processing message received on {QueueName}", eventArgs.EntityPath);
        return Task.CompletedTask;
    }
}
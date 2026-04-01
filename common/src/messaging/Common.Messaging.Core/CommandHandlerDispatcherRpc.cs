using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Common.Messaging.Core;

/// <summary>
/// Background service that listens for incoming RPC command messages on a Service Bus topic subscription
/// and dispatches them to the appropriate command handlers for processing.  After the handler is invoked,
/// the response is returned to the originating service on the reply queue.
/// </summary>
/// <param name="logger"></param>
/// <param name="serviceProvider"></param>
/// <param name="requestTopicProcessor"></param>
/// <param name="replyQueueSender"></param>
public class CommandHandlerDispatcherRpc(
    ILogger<CommandHandlerDispatcherRpc> logger,
    IServiceProvider serviceProvider,
    [FromKeyedServices("rpc")]ServiceBusProcessor requestTopicProcessor,
    [FromKeyedServices("rpc")]ServiceBusSender replyQueueSender) : BackgroundService
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
            var receivedCommand = ReceivedCommand.Create(eventArgs.Message);
            var commandRepository = requestScope.ServiceProvider.GetRequiredService<ICommandRepository>();
            var commandHandler = requestScope.ServiceProvider.GetCommandHandler(receivedCommand);
            
            receivedCommand.SetCommand(eventArgs.Message, commandHandler.CommandType);
            
            var response = await commandHandler.Handle(receivedCommand, commandRepository, eventArgs.CancellationToken);
            await SendReplyToCommand(commandHandler, response, eventArgs);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception processing message {MessageId}.", eventArgs.Message.MessageId);
        }
    }
    
    private async Task SendReplyToCommand(
        ICommandMessageHandler handler,
        CommandContext commandContext,
        ProcessMessageEventArgs eventArgs)
    {
        if (commandContext.Response is not null && eventArgs.Message.ReplyTo is not null)
        {
            var replyMessage = new ServiceBusMessage(JsonSerializer.SerializeToUtf8Bytes(commandContext.Response))
            {
                CorrelationId = eventArgs.Message.CorrelationId,
                SessionId = eventArgs.Message.SessionId,
            };
            
            await replyQueueSender.SendMessageAsync(replyMessage, eventArgs.CancellationToken);
            return;
        }
            
        if (commandContext.Response is null && eventArgs.Message.ReplyTo is not null)
        {
            logger.LogWarning(
                "Command handler {Handler} for message {MessageId} returned null but request message expected reply to queue {QueueName}.",
                handler,
                eventArgs.Message.MessageId,
                eventArgs.Message.ReplyTo);
                
            return;
        }

        if (commandContext.Response is not null && eventArgs.Message.ReplyTo is null)
        {
            logger.LogWarning(
                "Command handler {Handler} for message {MessageId} returned response but request didn't specify reply queue {QueueName}.",
                handler,
                eventArgs.Message.MessageId,
                eventArgs.Message.ReplyTo);
        }
    }
    
    private Task OnProcessErrorAsync(ProcessErrorEventArgs eventArgs)
    {
        logger.LogError(
            eventArgs.Exception,
            "Exception processing message received on {QueueName}", eventArgs.EntityPath);
        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping processing of RPC commands.");
        
        requestTopicProcessor.ProcessMessageAsync -= OnProcessMessageAsync;
        requestTopicProcessor.ProcessErrorAsync -= OnProcessErrorAsync;
        
        return base.StopAsync(cancellationToken);
    }
}
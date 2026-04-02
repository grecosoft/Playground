using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Common.Messaging.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Common.Messaging.Core.Commands;

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
    [FromKeyedServices("rpc-reply")]ServiceBusSender replyQueueSender) : BackgroundService
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
            var commandHandler = requestScope.ServiceProvider.GetCommandHandler(context);
            
            context.SetCommand(eventArgs.Message.Body, commandHandler.CommandType);
            
            await commandHandler.Handle(context, eventArgs.CancellationToken);
            await SendReplyToCommand(commandHandler, context, eventArgs);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception processing message {MessageId}.", eventArgs.Message.MessageId);
        }
    }
    
    private async Task SendReplyToCommand(
        ICommandMessageHandler handler,
        CommandContext commandContextContext,
        ProcessMessageEventArgs eventArgs)
    {
        if (commandContextContext.Response is not null && eventArgs.Message.ReplyTo is not null)
        {
            var replyMessage = new ServiceBusMessage(JsonSerializer.SerializeToUtf8Bytes(commandContextContext.Response))
            {
                CorrelationId = eventArgs.Message.CorrelationId,
                SessionId = eventArgs.Message.SessionId,
            };
            
            await replyQueueSender.SendMessageAsync(replyMessage, eventArgs.CancellationToken);
            return;
        }
            
        if (commandContextContext.Response is null && eventArgs.Message.ReplyTo is not null)
        {
            logger.LogWarning(
                "Command handler {Handler} for message {MessageId} returned null but request message expected reply to queue {QueueName}.",
                handler,
                eventArgs.Message.MessageId,
                eventArgs.Message.ReplyTo);
                
            return;
        }

        if (commandContextContext.Response is not null && eventArgs.Message.ReplyTo is null)
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
        return base.StopAsync(cancellationToken);
    }
}
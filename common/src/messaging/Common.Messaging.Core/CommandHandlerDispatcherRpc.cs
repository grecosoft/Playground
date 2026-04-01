using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Common.Messaging.Core;

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
            var commandHandler = ResolveCommandHandler(requestScope, receivedCommand);
            
            receivedCommand.SetCommand(eventArgs.Message, commandHandler.CommandType);
            
            switch (receivedCommand.DispatchStrategy)
            {
                case DispatchStrategyType.Rpc:
                    var response = await commandHandler.Handle(receivedCommand, commandRepository, eventArgs.CancellationToken);
                    await SendReplyToCommand(commandHandler, response, eventArgs);
                    break;
                case DispatchStrategyType.Async:
                    await commandHandler.Handle(receivedCommand, commandRepository, eventArgs.CancellationToken);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown command type of {receivedCommand.DispatchStrategy}");
            }
            
            // await eventArgs.CompleteMessageAsync(eventArgs.Message);
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

    private static ICommandMessageHandler ResolveCommandHandler(
        IServiceScope requestScope,
        ReceivedCommand receivedCommand)
    {
        var handler = requestScope.ServiceProvider.GetKeyedService<ICommandMessageHandler>(
            receivedCommand.CommandNamespace);

        return handler ?? throw new InvalidOperationException("");
    }
}
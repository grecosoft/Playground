using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Playground.Common.Messaging.Types;

namespace Playground.Common.Messaging.Services;

public class CommandHandlerDispatcher(
    ILogger<CommandHandlerDispatcher> logger,
    IServiceProvider serviceProvider,
    [FromKeyedServices("datalab.messaging.request")]ServiceBusProcessor requestQueueProcessor,
    [FromKeyedServices("datalab.messaging.reply")]ServiceBusSender replyQueueSender) : BackgroundService
{
    private const string CommandNamespacePropName = "command-namespace";
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        requestQueueProcessor.ProcessMessageAsync += OnProcessMessageAsync;
        requestQueueProcessor.ProcessErrorAsync += OnProcessErrorAsync;
        
        return requestQueueProcessor.StartProcessingAsync(stoppingToken);
    }

    private async Task OnProcessMessageAsync(ProcessMessageEventArgs eventArgs)
    {
        // Create service scope to execute the request within:
        using var requestScope = serviceProvider.CreateScope();

        if (!TryResolveCommandHandler(requestScope, eventArgs.Message, out var commandHandler))
        {
            logger.LogError("Message {MessageId} received on {QueueName} not processed.",
                eventArgs.Message.MessageId,
                requestQueueProcessor.EntityPath);
            return;
        }

        try
        {
            // Deserialize received message into the handler's associated command-type:
            var request = JsonSerializer.Deserialize(eventArgs.Message.Body, commandHandler.CommandType);
            if (request is null)
            {
                logger.LogError(
                    "Command message {MessageId} could not be deserialized into {CommandType} for handler {Handler}.",
                    eventArgs.Message.MessageId,
                    commandHandler.CommandType,
                    commandHandler);
                return;
            }
            
            
            // Invoke the handler to process the command:
            var response = await commandHandler.Handle((ICommandMessage)request, eventArgs.CancellationToken);
            
            await SendOptionalReplyToRequest(requestScope, commandHandler, response, eventArgs);
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

    private async Task SendOptionalReplyToRequest(
        IServiceScope requestScope,
        ICommandMessageHandler handler,
        object? response,
        ProcessMessageEventArgs eventArgs)
    {
        if (response is not null && eventArgs.Message.ReplyTo is not null)
        {
            var replyMessage = new ServiceBusMessage(JsonSerializer.SerializeToUtf8Bytes(response))
            {
                CorrelationId = eventArgs.Message.CorrelationId,
                SessionId = eventArgs.Message.SessionId,
            };
            
            var stopwatch = Stopwatch.StartNew();
            await replyQueueSender.SendMessageAsync(replyMessage, eventArgs.CancellationToken);
            
            stopwatch.Stop();
            Console.WriteLine($"Elapsed: {stopwatch.ElapsedMilliseconds} ms");
            return;
        }
            
        if (response is null && eventArgs.Message.ReplyTo is not null)
        {
            logger.LogWarning(
                "Command handler {Handler} for message {MessageId} returned null but request message expected reply to queue {QueueName}.",
                handler,
                eventArgs.Message.MessageId,
                eventArgs.Message.ReplyTo);
                
            return;
        }

        if (response is not null && eventArgs.Message.ReplyTo is null)
        {
            logger.LogWarning(
                "Command handler {Handler} for message {MessageId} returned response but request didn't specify reply queue {QueueName}.",
                handler,
                eventArgs.Message.MessageId,
                eventArgs.Message.ReplyTo);
        }
    }
    
    // The handler responsible for processing the received command-message is determined by the 
    // namespace message property.  Each ICommandMessageHandler is registered as a keyed service
    // based on the namespace associated with the command it handles.
    private bool TryResolveCommandHandler(IServiceScope requestScope, ServiceBusReceivedMessage message,
        [NotNullWhen(true)] out ICommandMessageHandler? handler)
    {
        handler = null;
        if (!message.ApplicationProperties.TryGetValue(CommandNamespacePropName, out var commandNamespace))
        {
            logger.LogError(
                "Command message {MessageId} received on {QueueName} missing {ApplicationProperty}.", 
                message.MessageId,
                requestQueueProcessor.EntityPath,
                CommandNamespacePropName);
            return false;
        }
        
        handler = requestScope.ServiceProvider.GetKeyedService<ICommandMessageHandler>(commandNamespace);
        if (handler is not null) return true;
        
        logger.LogError(
            "Command handler for {Namespace} not registered for command message {MessageId} received on {QueueName}.",
            commandNamespace,
            message.MessageId,
            requestQueueProcessor.EntityPath);
        return false;
    }
}
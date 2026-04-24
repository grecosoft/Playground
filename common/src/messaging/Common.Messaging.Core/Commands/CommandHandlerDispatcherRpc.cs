using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Common.Messaging.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Common.Messaging.Core.Commands;

/// <summary>
/// Background service that listens for incoming RPC command messages on a Service Bus topic subscription
/// and dispatches them to the appropriate command handlers for processing.  After the handler is invoked,
/// the response is returned to the originating service on the reply queue.
/// </summary>
/// <param name="messagingOptions">Messaging related configurations.</param>
/// <param name="logger">Logger.</param>
/// <param name="serviceProvider">Service provider used to create scope to execute handler within.</param>
/// <param name="requestTopicProcessor">The processor on which the commands are received.</param>
/// <param name="replyQueueSender">Used to sent replay to command back to calling service.</param>
public class CommandHandlerDispatcherRpc(
    IOptions<MessagingConfig> messagingOptions,
    ILogger<CommandHandlerDispatcherRpc> logger,
    IServiceProvider serviceProvider,
    [FromKeyedServices("rpc")]ServiceBusProcessor requestTopicProcessor,
    [FromKeyedServices("rpc-reply")]ServiceBusSender replyQueueSender) : BackgroundService
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
            
            logger.LogDebug(
                "Dispatching command message {MessageId} to handler {Handler} of type {CommandType}.",
                eventArgs.Message.MessageId, 
                commandHandler,
                commandHandler.CommandType);
            
            // Call the handler to process the command, and then send the response
            // back to the caller on the reply queue:
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
        CommandContext context,
        ProcessMessageEventArgs eventArgs)
    {
        if (context.Response is null)
        {
            throw new InvalidOperationException(
                $"Command handler {handler.GetType().Name} did not set a response on the command context.");
        }
        
        var payload = new CommandPayload(
            context.SendingServiceId,
            context.SendingServiceName,
            BinaryData.FromObjectAsJson(context.Command),
            BinaryData.FromObjectAsJson(context.Response));
        
        var replyMessage = new ServiceBusMessage(JsonSerializer.SerializeToUtf8Bytes(payload))
        {
            CorrelationId = eventArgs.Message.CorrelationId,
            SessionId = eventArgs.Message.SessionId,
        };
            
        await replyQueueSender.SendMessageAsync(replyMessage, eventArgs.CancellationToken);
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
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Common.Messaging.Commands;
using Common.Messaging.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Common.Messaging.Core.Commands;

public class CommandMessagingService(
    IOptions<MessagingConfig> messagingOptions,
    ILogger<CommandMessagingService> logger,
    ServiceBusClient client,
    [FromKeyedServices("rpc")] ServiceBusSender rpcCommandSender,
    [FromKeyedServices("async")] ServiceBusSender asyncCommandSender) : ICommandMessagingService
{
    // Caches message associated metadata used to set properties 
    // when publishing message to the consuming service.
    private readonly ConcurrentDictionary<Type, MessageMetadata> _messageMetadata = new();
    private readonly MessagingConfig _busConfig = messagingOptions.Value;

    // Sends a command to a destination service and waits for a max specific
    // amount of time for a response before timing out.
    public async Task<TResponse> SendCommandWithReplyAsync<TResponse>(
        ICommandMessage command,
        EndpointInfo endpointInfo,
        CancellationToken token)
    {
        var correlationId = await SendCommandMessage(
            rpcCommandSender,
            endpointInfo,
            command,
            token);

        return await WaitCommandResponse<TResponse>(correlationId, endpointInfo, token);
    }

    // Sends a command to a destination service and expects a response to be sent back to the reply queue, but does not
    // wait for the response.  This is useful for fire-and-forget style commands.  Also, response may never be received.
    public Task SendCommandAsync<TResponse>(
        ICommandMessage<TResponse> command,
        EndpointInfo endpointInfo,
        CancellationToken token)
    {
        return SendCommandMessage(
            asyncCommandSender,
            endpointInfo,
            command,
            token);
    }

    // Sends a response to a previously received command. This is used by the service that receives the command and is \
    // processing it and wants to send a response sometime in the future.
    public async Task SendResponseToCommandAsync<TResponse>(
        CommandContext context,
        ICommandMessage<TResponse> command,
        CancellationToken token)
    {
        var message = new ServiceBusMessage(JsonSerializer.SerializeToUtf8Bytes(command, command.GetType()))
        {
            CorrelationId = context.CorrelationId,
            ApplicationProperties =
            {
                { "service_id", context.SendingServiceId }
            }
        };

        message.ApplicationProperties.Add(MessageProperties.CommandNamespace, context.CommandNamespace);
        message.ApplicationProperties.Add(MessageProperties.SendingServiceId, _busConfig.ServiceId);

        await asyncCommandSender.SendMessageAsync(message, token);
    }

    private async Task<string> SendCommandMessage(
        ServiceBusSender sender,
        EndpointInfo endpointInfo,
        ICommandMessage command,
        CancellationToken token)
    {
        var correlationId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString();
        var messageMetadata = GetMessageMetadata(command.GetType());
        var message = new ServiceBusMessage(JsonSerializer.SerializeToUtf8Bytes(command, command.GetType()))
        {
            CorrelationId = correlationId,
            SessionId = correlationId,
            ReplyTo = _busConfig.RpcReplyQueue,
            ApplicationProperties =
            {
                { MessageProperties.EndpointServiceId, endpointInfo.ServiceId },
                { MessageProperties.CommandNamespace, messageMetadata.MessageNamespace },
                { MessageProperties.SendingServiceId, _busConfig.ServiceId }
            }
        };
        
        logger.LogDebug(
            "Sending command message for {Namespace} with correlation id {CorrelationId} to service {DestinationServiceId}",
            messageMetadata.MessageNamespace,
            message.CorrelationId,
            endpointInfo.ServiceId);

        try
        {
            await sender.SendMessageAsync(message, token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Sending command message for {Namespace} with correlation id {CorrelationId} to service {DestinationServiceId}",
                messageMetadata.MessageNamespace,
                message.CorrelationId,
                endpointInfo.ServiceId);
        }
        
        return correlationId;
    }

    private async Task<TResponse> WaitCommandResponse<TResponse>(string correlationId, EndpointInfo endpointInfo, CancellationToken token)
    {
        logger.LogDebug(
            "Waiting for response for {CorrelationId} from service {DestinationServiceId}", 
            correlationId, 
            endpointInfo.ServiceId);
        
        await using var sessionReceiver = await client.AcceptSessionAsync(
            _busConfig.RpcReplyQueue,
            correlationId,
            new ServiceBusSessionReceiverOptions { ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete },
            token);

        var replyMessage = await sessionReceiver.ReceiveMessageAsync(
            TimeSpan.FromSeconds(_busConfig.RpcReplyTimeoutSeconds),
            token);

        if (replyMessage == null)
        {
            logger.LogError(
                "Timed out waiting for response for {CorrelationId} from service {DestinationServiceId} after {TimeoutSeconds} seconds",
                correlationId,
                endpointInfo.ServiceId,
                _busConfig.RpcReplyTimeoutSeconds);
            
            throw new TimeoutException(
                $"Timed out waiting for response for {correlationId} from service {endpointInfo.ServiceId} " + 
                $"after {_busConfig.RpcReplyTimeoutSeconds} seconds");
        }
        
        logger.LogDebug(
            "Received response for {CorrelationId} from service {DestinationServiceId}", 
            correlationId, 
            endpointInfo.ServiceId);

        var response = JsonSerializer.Deserialize<TResponse>(replyMessage.Body);
        return response ?? throw new InvalidOperationException(
            $"Failed to deserialize response message body to type '{typeof(TResponse).Name}'.");
    }

    private MessageMetadata GetMessageMetadata(Type messageType)
    {
        return _messageMetadata.GetOrAdd(messageType, t =>
        {
            var attrib = t.GetCustomAttribute<MessageNamespace>();
            return attrib == null
                ? throw new InvalidOperationException($"The message namespace '{t.Name}' is not found.")
                : new MessageMetadata(attrib.NamespaceName);
        });
    }

    private record MessageMetadata(string MessageNamespace);
}
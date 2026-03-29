using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Playground.Common.Messaging.Types;

namespace Playground.Common.Messaging.Services;

public class MessagingService(
    BusMessagingConfig busConfig,
    ServiceBusClient client,
    ServiceBusSender sender) : IMessagingService
{
    // Caches message associated metadata used to set properties 
    // when publishing message to the consuming service.
    private readonly ConcurrentDictionary<Type, MessageMetadata> _messageMetadata = new();
    
    public async Task<TResponse> SendCommandWithReplyAsync<TResponse>(
        ICommandMessage command,
        EndpointInfo endpointInfo,
        CancellationToken token)
    {
        var correlationId = Guid.NewGuid().ToString();
        await SendCommandMessage(
            correlationId, 
            endpointInfo,
            command,
            new Dictionary<string, object>
            {
                { MessageProperties.DispatchStrategyType, nameof(DispatchStrategyType.Rpc) }
            },
            token);
        return await WaitCommandResponse<TResponse>(correlationId, token);
    }
    
    public Task SendCommandAsync<TResponse>(
        ICommandMessage<TResponse> command,
        EndpointInfo endpointInfo,
        CancellationToken token)
    {
        var correlationId = Guid.NewGuid().ToString();
        return SendCommandMessage(
            correlationId,
            endpointInfo,
            command,
            new Dictionary<string, object>
            {
                { MessageProperties.DispatchStrategyType, nameof(DispatchStrategyType.Async) }
            },
            token);
    }
    
    public async Task SendResponseToCommandAsync<TResponse>(
        ReceivedCommand receivedCommand,
        ICommandMessage<TResponse> command,
        CancellationToken token)
    {
        var message = new ServiceBusMessage(JsonSerializer.SerializeToUtf8Bytes(command, command.GetType()))
        {
            CorrelationId = receivedCommand.CorrelationId,
            ApplicationProperties =
            {
                { "service", receivedCommand.ReplyToServiceId }
            }
        };

        message.ApplicationProperties.Add(MessageProperties.CommandNamespace, receivedCommand.CommandNamespace);
        message.ApplicationProperties.Add(MessageProperties.DispatchStrategyType, nameof(DispatchStrategyType.Async));
        message.ApplicationProperties.Add(MessageProperties.SendingServiceId, busConfig.ServiceId);
        
        await sender.SendMessageAsync(message, token);
    }
    
    private async Task SendCommandMessage(
        string correlationId,
        EndpointInfo endpointInfo,
        ICommandMessage command, 
        IDictionary<string, object> messageProperties,
        CancellationToken token)
    {
        var messageMetadata = GetMessageMetadata(command.GetType());
        var message = new ServiceBusMessage(JsonSerializer.SerializeToUtf8Bytes(command, command.GetType()))
        {
            CorrelationId = correlationId,
            SessionId = correlationId,
            ReplyTo = busConfig.SolutionReplyQueue,
            ApplicationProperties =
            {
                { "service", endpointInfo.ServiceId }
            }
        };

        message.ApplicationProperties.Add(MessageProperties.CommandNamespace, messageMetadata.MessageNamespace);
        message.ApplicationProperties.Add(MessageProperties.SendingServiceId, busConfig.ServiceId);
        
        foreach (var property in messageProperties)
        {
            message.ApplicationProperties.Add(property.Key, property.Value);
        }
        
        await sender.SendMessageAsync(message, token);
    }
    
    private async Task<TResponse> WaitCommandResponse<TResponse>(string correlationId, CancellationToken token)
    {
        await using var sessionReceiver = await client.AcceptSessionAsync(
            busConfig.SolutionReplyQueue, 
            correlationId,
            new ServiceBusSessionReceiverOptions { ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete },
            token);
        
        var replyMessage = await sessionReceiver.ReceiveMessageAsync(
            TimeSpan.FromSeconds(busConfig.ReplyTimeoutSeconds),
            token);

        if (replyMessage == null)
        {
            throw new TimeoutException("");
        }
        
        var response = JsonSerializer.Deserialize<TResponse>(replyMessage.Body);
        return response ?? throw new InvalidOperationException("");
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
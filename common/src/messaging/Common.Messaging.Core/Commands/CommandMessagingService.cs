using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Common.Messaging.Commands;
using Common.Messaging.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Common.Messaging.Core.Commands;

public class CommandMessagingService(
    IOptions<MessagingConfig> messagingOptions,
    ServiceBusClient client,
    [FromKeyedServices("rpc")] ServiceBusSender rpcCommandSender,
    [FromKeyedServices("async")] ServiceBusSender asyncCommandSender) : ICommandMessagingService
{
    // Caches message associated metadata used to set properties 
    // when publishing message to the consuming service.
    private readonly ConcurrentDictionary<Type, MessageMetadata> _messageMetadata = new();
    private readonly MessagingConfig _busConfig = messagingOptions.Value;
    
    public async Task<TResponse> SendCommandWithReplyAsync<TResponse>(
        ICommandMessage command,
        EndpointInfo endpointInfo,
        CancellationToken token)
    {
        var correlationId = Guid.NewGuid().ToString();
        await SendCommandMessage(
            rpcCommandSender,
            correlationId, 
            endpointInfo,
            command,
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
            asyncCommandSender,
            correlationId,
            endpointInfo,
            command,
            token);
    }
    
    public async Task SendResponseToCommandAsync<TResponse>(
        CommandContext commandContext,
        ICommandMessage<TResponse> command,
        CancellationToken token)
    {
        var message = new ServiceBusMessage(JsonSerializer.SerializeToUtf8Bytes(command, command.GetType()))
        {
            CorrelationId = commandContext.CorrelationId,
            ApplicationProperties =
            {
                { "service_id", commandContext.ReplyToServiceId }
            }
        };

        message.ApplicationProperties.Add(MessageProperties.CommandNamespace, commandContext.CommandNamespace);
        message.ApplicationProperties.Add(MessageProperties.SendingServiceId, _busConfig.ServiceId);
        
        await asyncCommandSender.SendMessageAsync(message, token);
    }
    
    private async Task SendCommandMessage(
        ServiceBusSender sender,
        string correlationId,
        EndpointInfo endpointInfo,
        ICommandMessage command, 
        CancellationToken token)
    {
        var messageMetadata = GetMessageMetadata(command.GetType());
        var message = new ServiceBusMessage(JsonSerializer.SerializeToUtf8Bytes(command, command.GetType()))
        {
            CorrelationId = correlationId,
            SessionId = correlationId,
            ReplyTo = _busConfig.RpcReplyQueue,
            ApplicationProperties =
            {
                { "service_id", endpointInfo.ServiceId }
            }
        };

        message.ApplicationProperties.Add(MessageProperties.CommandNamespace, messageMetadata.MessageNamespace);
        message.ApplicationProperties.Add(MessageProperties.SendingServiceId, _busConfig.ServiceId);
        
        await sender.SendMessageAsync(message, token);
    }
    
    private async Task<TResponse> WaitCommandResponse<TResponse>(string correlationId, CancellationToken token)
    {
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
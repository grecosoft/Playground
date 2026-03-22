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
    
    public async Task<TResponse> SendCommandWithReplyAsync<TResponse>(ICommandMessage<TResponse> command, 
        CancellationToken token) 
    {
        var correlationId = Guid.NewGuid().ToString();
       
        await SendCommandMessage(correlationId, command, token);
        return await WaitCommandResponse<TResponse>(correlationId, token);
    }

    public Task SendCommandWithResponseAsync<TResponse>(ICommandMessage<TResponse> command, CancellationToken token)
    {
        var correlationId = Guid.NewGuid().ToString();
        return SendCommandMessage(correlationId, command, token);
    }

    private async Task SendCommandMessage(string correlationId, ICommandMessage command, CancellationToken token)
    {
        var messageMetadata = GetMessageMetadata(command.GetType());
        var message = new ServiceBusMessage(JsonSerializer.SerializeToUtf8Bytes(command, command.GetType()))
        {
            CorrelationId = correlationId,
            SessionId = correlationId,
            ReplyTo = busConfig.SolutionReplyQueue,
            ApplicationProperties =
            {
                { "service", messageMetadata.TargetService }
            }
        };

        message.ApplicationProperties.Add(MessageProperties.CommandNamespace, messageMetadata.MessageNamespace);
        message.ApplicationProperties.Add(MessageProperties.DispatchStrategyType, messageMetadata.DispatchStrategyType);
        message.ApplicationProperties.Add(MessageProperties.SendingService, busConfig.ServiceName);
        
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
                : new MessageMetadata(
                    t,
                    busConfig.SolutionName,
                    attrib.ServiceName,
                    attrib.NamespaceName, 
                    attrib.CommandType.ToString());
        });
    }
    
    private record MessageMetadata(
        Type MessageType,
        string SolutionName,
        string ServiceName,
        string MessageNamespace,
        string DispatchStrategyType)
    {
        public string TargetService =>  $"{SolutionName}:{ServiceName}";
    }
}
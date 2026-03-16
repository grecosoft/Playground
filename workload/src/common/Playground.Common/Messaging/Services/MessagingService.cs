using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Playground.Common.Messaging.Types;

namespace Playground.Common.Messaging.Services;

public class MessagingService(
    string solutionName,
    BusMessagingConfig busConfig,
    ServiceBusClient client,
    ServiceBusSender sender) : IMessagingService
{
    private readonly ConcurrentDictionary<Type, MessageMetadata> _messageMetadata = new();
    
    public async Task<TResponse> SendAsync<TResponse>(ICommandMessage<TResponse> command, 
        CancellationToken token) 
    {
        var correlationId = Guid.NewGuid().ToString();
       
        await SendCommandMessage(correlationId, command, token);
        return await WaitCommandResponse<TResponse>(correlationId, token);
    }
    
    private async Task SendCommandMessage(string correlationId, ICommandMessage command, CancellationToken token)
    {
        var messageMetadata = GetMessageMetadata(command.GetType());
        var message = new ServiceBusMessage(JsonSerializer.SerializeToUtf8Bytes(command, command.GetType()))
        {
            CorrelationId = correlationId,
            SessionId = correlationId,
            ReplyTo = busConfig.ReplyQueue,
            ApplicationProperties =
            {
                { "service", messageMetadata.TargetService }
            }
        };

        message.ApplicationProperties.Add(
            "command-namespace",
            messageMetadata.MessageNamespace);
        
        await sender.SendMessageAsync(message, token);
    }

    private async Task<TResponse> WaitCommandResponse<TResponse>(string correlationId, CancellationToken token)
    {
        await using var sessionReceiver = await client.AcceptSessionAsync(
            busConfig.ReplyQueue, 
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
        if (response == null)
        {
            throw new InvalidOperationException("");
        }
        
        return response;
    }

    public Task SendAsync(ICommandMessage command, CancellationToken token) 
    {
        throw new NotImplementedException();
    }
    
    private MessageMetadata GetMessageMetadata(Type messageType)
    {
        return _messageMetadata.GetOrAdd(messageType, t =>
        {
            var attrib = t.GetCustomAttribute<MessageNamespace>();
            return attrib == null 
                ? throw new InvalidOperationException($"The message namespace '{t.Name}' is not found.")
                : new MessageMetadata(t, solutionName, attrib.ServiceName, attrib.NamespaceName);
        });
    }
    
    private record MessageMetadata(Type MessageType, string SolutionName, string ServiceName, string MessageNamespace)
    {
        public string TargetService =>  $"{SolutionName}:{ServiceName}";
    }
}
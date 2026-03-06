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
    public async Task<TResponse> SendAsync<TResponse>(ICommandMessage<TResponse> command, 
        CancellationToken token) 
    {
        var correlationId = Guid.NewGuid().ToString();
       
        await SendCommandMessage(correlationId, command, token);
        return await WaitCommandResponse<TResponse>(correlationId, token);
    }

    private async Task SendCommandMessage(string correlationId, ICommandMessage command, CancellationToken token)
    {
        var message = new ServiceBusMessage(JsonSerializer.SerializeToUtf8Bytes(command, command.GetType()))
        {
            CorrelationId = correlationId,
            SessionId = correlationId,
            ReplyTo = busConfig.ReplyQueue
        };

        message.ApplicationProperties.Add(
            "command-namespace",
            command.GetType().GetCustomAttribute<CommandNamespace>()!.NamespaceName);
        
        await sender.SendMessageAsync(message, token);
    }

    private async Task<TResponse> WaitCommandResponse<TResponse>(string correlationId, CancellationToken token)
    {
        await using var sessionReceiver = await client.AcceptSessionAsync(
            busConfig.ReplyQueue, 
            correlationId,
            new ServiceBusSessionReceiverOptions { ReceiveMode =ServiceBusReceiveMode.ReceiveAndDelete },
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
}
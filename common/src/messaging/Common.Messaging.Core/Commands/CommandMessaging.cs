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

public class CommandMessaging: ICommandMessaging
{
    /// <summary>
    /// Core implementation of the command messaging system.  This class is responsible for sending commands
    /// to other services and sending responses back to calling services for previously received commands.
    /// When the CommandMessaging class is instantiated, it creates CommandEndpoint instances for each dependent
    /// service configured in the MessagingConfig. The CommandEndpoint instances are used to send commands to the
    /// appropriate service endpoints.
    /// </summary>
    /// <param name="messagingOptions">Messaging related configurations.</param>
    /// <param name="logger">Configured logger.</param>
    /// <param name="commandRepository">Repository containing pending commands.</param>
    /// <param name="client">Reference to the Service Bus Client.</param>
    /// <param name="rpcCommandSender">Used to send RPC style of commands.</param>
    /// <param name="asyncCommandSender">Used to send asynchronous commands having future responses.</param>
    public CommandMessaging( 
        IOptions<MessagingConfig> messagingOptions,
        ILogger<CommandMessaging> logger,
        ICommandRepository commandRepository,
        ServiceBusClient client,
        [FromKeyedServices("rpc")] ServiceBusSender rpcCommandSender,
        [FromKeyedServices("async")] ServiceBusSender asyncCommandSender)
    {
        _busConfig = messagingOptions.Value;
        _logger = logger;
        _commandRepository = commandRepository;
        
        _client = client;
        _rpcCommandSender = rpcCommandSender;
        _asyncCommandSender = asyncCommandSender;
        
        AddCommandEndpoints(_busConfig);
    }
    
    private readonly MessagingConfig _busConfig;
    private readonly ILogger<CommandMessaging> _logger;
    private readonly ICommandRepository _commandRepository;
    
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _rpcCommandSender;
    private readonly ServiceBusSender _asyncCommandSender;

    private readonly Dictionary<string, CommandEndpoint> _commandEndpoints = new();
    
    // Caches message associated metadata used to set properties 
    // when publishing message to the consuming service.
    private readonly ConcurrentDictionary<Type, MessageMetadata> _messageMetadata = new();
    
    public ICommandEndpoint GetServiceEndpoint(string serviceName)
    {
        return _commandEndpoints.TryGetValue(serviceName, out var commandEndpoint) 
             ? commandEndpoint : throw new InvalidOperationException($"Service '{serviceName}' is not registered.");
    }
    
    private void AddCommandEndpoints(MessagingConfig messagingConfig)
    {
        foreach (var dependentService in messagingConfig.DependentServices)
        {
            var endpointInfo = new EndpointInfo(dependentService.Key, dependentService.Value);
            _commandEndpoints.Add(dependentService.Key, new CommandEndpoint(endpointInfo, this));
        }
    }
    
    // Sends a response to a previously received command back to the originating service.  This method is used
    // within a service that receives commands, and needs to send a response back to the caller of the command.
    // The caller of the command is not expected to be waiting for an immediate response,
    public async Task SendResponseToCommandAsync<TResponse>(
        CommandContext context,
        ICommandMessage<TResponse> command,
        CancellationToken ct)
    {
        var message = new ServiceBusMessage(JsonSerializer.SerializeToUtf8Bytes(command, command.GetType()))
        {
            CorrelationId = context.CorrelationId,
            ApplicationProperties =
            {
                // Specified so the command response is delivered back to the service that originally send the command.
                { MessageProperties.EndpointServiceId, context.SendingServiceId },
                
                // Message metadata:
                { MessageProperties.CommandNamespace, context.CommandNamespace },
                { MessageProperties.SendingServiceId, _busConfig.ServiceId }
            }
        };
        
        await _asyncCommandSender.SendMessageAsync(message, ct);
        await _commandRepository.DeleteCommand(context.CorrelationId, ct);
    }
    
    // Sends a command to a destination service and waits for a max specific
    // amount of time for a response before timing out.
    internal async Task<TResponse> SendCommandWithReplyAsync<TResponse>(
        ICommandMessage command,
        EndpointInfo endpointInfo,
        CancellationToken ct)
    {
        var correlationId = await SendCommandMessage(
            _rpcCommandSender,
            endpointInfo,
            command,
            ct);

        return await WaitCommandResponse<TResponse>(correlationId, endpointInfo, ct);
    }

    // Sends a command to a destination service and expects a response to be sent back on the reply queue, but does not
    // wait for the response.  
    internal Task SendCommandAsync<TResponse>(
        ICommandMessage<TResponse> command,
        EndpointInfo endpointInfo,
        CancellationToken token)
    {
        return SendCommandMessage(
            _asyncCommandSender,
            endpointInfo,
            command,
            token);
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
        
        _logger.LogDebug(
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
            _logger.LogError(ex,
                "Sending command message for {Namespace} with correlation id {CorrelationId} to service {DestinationServiceId}",
                messageMetadata.MessageNamespace,
                message.CorrelationId,
                endpointInfo.ServiceId);
        }
        
        return correlationId;
    }

    private async Task<TResponse> WaitCommandResponse<TResponse>(
        string correlationId,
        EndpointInfo endpointInfo,
        CancellationToken ct)
    {
       _logger.LogDebug(
            "Waiting for response for {CorrelationId} from service {DestinationServiceId}", 
            correlationId, 
            endpointInfo.ServiceId);
        
        await using var sessionReceiver = await _client.AcceptSessionAsync(
            _busConfig.RpcReplyQueue,
            correlationId,
            new ServiceBusSessionReceiverOptions { ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete },
            ct);

        var replyMessage = await sessionReceiver.ReceiveMessageAsync(
            TimeSpan.FromSeconds(_busConfig.RpcReplyTimeoutSeconds),
            ct);

        if (replyMessage == null)
        {
            _logger.LogError(
                "Timed out waiting for response for {CorrelationId} from service {DestinationServiceId} after {TimeoutSeconds} seconds",
                correlationId,
                endpointInfo.ServiceId,
                _busConfig.RpcReplyTimeoutSeconds);
            
            throw new TimeoutException(
                $"Timed out waiting for response for {correlationId} from service {endpointInfo.ServiceId} " + 
                $"after {_busConfig.RpcReplyTimeoutSeconds} seconds");
        }
        
        _logger.LogDebug(
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
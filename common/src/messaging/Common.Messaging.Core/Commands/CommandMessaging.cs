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
    /// Core implementation of the command messaging. This class is responsible for sending commands to
    /// other services and sending responses back to calling services for previously received commands.
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
        _msgConfig = messagingOptions.Value;
        _logger = logger;
        _commandRepository = commandRepository;
        
        _client = client;
        _rpcCommandSender = rpcCommandSender;
        _asyncCommandSender = asyncCommandSender;
        
        AddCommandEndpoints(_msgConfig);
    }
    
    private readonly MessagingConfig _msgConfig;
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
    public async Task SendResponseToCommandAsync(
        CommandContext context,
        CancellationToken ct)
    {
        var payload = CreateCommandPayload(context);
        
        var message = new ServiceBusMessage(JsonSerializer.SerializeToUtf8Bytes(payload))
        {
            CorrelationId = context.CorrelationId,
            ApplicationProperties =
            {
                // Specified so the command response is delivered back to the service that originally send the command.
                { MessageProperties.EndpointServiceId, context.SendingServiceId },
                
                // Message metadata:
                { MessageProperties.CommandNamespace, context.CommandNamespace },
                { MessageProperties.SendingServiceId, _msgConfig.ServiceId }
            }
        };

        try
        {
            _logger.LogDebug(
                "Sending Response: ({Source}=>[{Namespace}:{CorrelationId}])", 
                _msgConfig.ServiceName,
                context.CommandNamespace,
                context.CorrelationId);
            
            await _asyncCommandSender.SendMessageAsync(message, ct);
            await _commandRepository.DeleteCommandCommand(context.CorrelationId, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send response for command with correlation id {CorrelationId} to service {ServiceId}.",
                context.CorrelationId,
                context.SendingServiceId);
        }
    }
    
    private static CommandPayload CreateCommandPayload(CommandContext context)
    {
        // This is the case if the response originates from an external client for which the
        // command type used by internal services isn't known.
        if (!context.ResponseData.IsEmpty)
        {
            return new CommandPayload(context.CommandData, context.ResponseData);
        }
        
        // This is the case when the response originates from an internal service that has access to the command type,
        // and can serialize the response based on the command handler's response type.
        return new CommandPayload(
            BinaryData.FromBytes(JsonSerializer.SerializeToUtf8Bytes(context.Command, context.Command.GetType())),
            BinaryData.FromObjectAsJson(context.Response));
    }
    
    // Sends a command to a destination service and waits for a max specific
    // amount of time for a response before timing out.
    internal async Task<TResponse> SendCommandWithReplyAsync<TResponse>(
        ICommandMessage command,
        EndpointInfo endpointInfo,
        CancellationToken ct)
    {
        var messageMetadata = GetMessageMetadata(command.GetType());
        
        var correlationId = await SendCommandMessage(
            _rpcCommandSender,
            endpointInfo,
            messageMetadata,
            command,
            ct);

        return await WaitCommandResponse<TResponse>(correlationId, endpointInfo, messageMetadata, ct);
    }
    
    private async Task<TResponse> WaitCommandResponse<TResponse>(
        string correlationId,
        EndpointInfo endpointInfo,
        MessageMetadata messageMetadata,
        CancellationToken ct)
    {
       _logger.LogDebug(
            "Waiting Response: [{Destination}<={Source}]({Namespace}:{CorrelationId})", 
            _msgConfig.ServiceName,
            messageMetadata.MessageNamespace,
            correlationId, 
            endpointInfo.ServiceName);
        
        await using var sessionReceiver = await _client.AcceptSessionAsync(
            _msgConfig.RpcReplyQueue,
            correlationId,
            new ServiceBusSessionReceiverOptions { ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete },
            ct);

        var replyMessage = await sessionReceiver.ReceiveMessageAsync(
            TimeSpan.FromSeconds(_msgConfig.RpcReplyTimeoutSeconds),
            ct);

        if (replyMessage == null)
        {
            _logger.LogError(
                "Timed out waiting for response for {CorrelationId} from service {DestinationServiceId} after {TimeoutSeconds} seconds",
                correlationId,
                endpointInfo.ServiceId,
                _msgConfig.RpcReplyTimeoutSeconds);
            
            throw new TimeoutException(
                $"Timed out waiting for response for {correlationId} from service {endpointInfo.ServiceId} " + 
                $"after {_msgConfig.RpcReplyTimeoutSeconds} seconds");
        }
        
        _logger.LogDebug(
            "[{Destination}<={Source}]({Namespace}:{CorrelationId})", 
            _msgConfig.ServiceName,
            messageMetadata.MessageNamespace,
            correlationId, 
            endpointInfo.ServiceName);
        
        var payload = JsonSerializer.Deserialize<CommandPayload>(replyMessage.Body) 
            ?? throw new InvalidOperationException("Failed to deserialize CommandPayload.");

        var response = JsonSerializer.Deserialize<TResponse>(payload.Response);
        return response ?? throw new InvalidOperationException(
            $"Failed to deserialize response message body to type '{typeof(TResponse).Name}'.");
    }

    // Sends a command to a destination service and expects a response to be sent back on the reply queue, but does not
    // wait for the response.  
    internal Task SendCommandAsync<TResponse>(
        ICommandMessage<TResponse> command,
        EndpointInfo endpointInfo,
        CancellationToken token)
    {
        var messageMetadata = GetMessageMetadata(command.GetType());
        
        return SendCommandMessage(
            _asyncCommandSender,
            endpointInfo,
            messageMetadata,
            command,
            token);
    }
    
    private async Task<string> SendCommandMessage(
        ServiceBusSender sender,
        EndpointInfo endpointInfo,
        MessageMetadata messageMetadata,
        ICommandMessage command,
        CancellationToken token)
    {
        var correlationId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString();
      
        var payload = new CommandPayload(
            BinaryData.FromBytes(JsonSerializer.SerializeToUtf8Bytes(command, command.GetType())),
            BinaryData.Empty);
        
        var message = new ServiceBusMessage(JsonSerializer.SerializeToUtf8Bytes(payload))
        {
            CorrelationId = correlationId,
            SessionId = correlationId,
            ReplyTo = _msgConfig.RpcReplyQueue,
            ApplicationProperties =
            {
                { MessageProperties.EndpointServiceId, endpointInfo.ServiceId },
                { MessageProperties.CommandNamespace, messageMetadata.MessageNamespace },
                { MessageProperties.SendingServiceId, _msgConfig.ServiceId }
            }
        };
        
        _logger.LogDebug(
            "Sending Command: [{Source}=>{Destination}]({Namespace}:{CorrelationId})",
            _msgConfig.ServiceName,
            messageMetadata.MessageNamespace,
            message.CorrelationId,
            endpointInfo.ServiceName);

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
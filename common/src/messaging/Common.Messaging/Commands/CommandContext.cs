using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Common.Messaging.Entities;

namespace Common.Messaging.Commands;

/// <summary>
/// Represents a received command and associated metadata passed to command handlers. 
/// </summary>
/// <param name="correlationId">Unique identity used to identify the command.</param>
/// <param name="sendingServiceId">The identity of the service sending the command.</param>
/// <param name="sendingServiceName">A friendly name for the service sending the command used for logging.</param>
/// <param name="commandNamespace">A hierarchical period separated value identifying the command.</param>
public class CommandContext(
    string correlationId,
    string sendingServiceId,
    string sendingServiceName,
    string commandNamespace)
{
    private ICommandMessage? _command;
    private ICommandRepository? _commandRepository;

    /// <summary>
    /// Unique identity used to identify the command.
    /// </summary>
    public string CorrelationId { get; } = correlationId;
    
    /// <summary>
    /// The identity of the service sending the command.
    /// </summary>
    public string SendingServiceId { get; } = sendingServiceId;
    
    /// <summary>
    /// A friendly name for the service sending the command used for logging.
    /// </summary>
    public string SendingServiceName { get; } = sendingServiceName;
    
    /// <summary>
    /// A hierarchical period separated value identifying the command.
    /// </summary>
    public string CommandNamespace { get; } = commandNamespace;

    public ICommandMessage Command => _command ?? throw new InvalidOperationException("Command not set.");
    public ICommandRepository CommandRepository => _commandRepository ?? throw new InvalidOperationException("CommandRepository not set.");

    public BinaryData CommandData { get; private set; } = BinaryData.Empty;
    public BinaryData ResponseData { get; private set; } = BinaryData.Empty;

    public Type CommandType => Command.GetType();
    public object? Response { get; private set; }
    public string? ResponseError { get; private set; }

    public static CommandContext Create(ServiceBusReceivedMessage message)
    {
        return new CommandContext(
            message.CorrelationId,
            message.GetRequiredStringProperty(MessageProperties.SendingServiceId),
            message.GetRequiredStringProperty(MessageProperties.SendingServiceName),
            message.GetRequiredStringProperty(MessageProperties.CommandNamespace));
    }
    
    public void SetCommand(BinaryData data, Type commandType)
    {
        if (_command is not null)
            throw new InvalidOperationException("Command has already been set.");

        CommandData = data;
        
        _command = (ICommandMessage)(
            JsonSerializer.Deserialize(data, commandType)
            ?? throw new InvalidOperationException($"Failed to deserialize message body to type '{commandType.Name}'.")
        );
    }
    
    public void SetResponse(object response)
    {
        if (Response is not null)
            throw new InvalidOperationException("Response has already been set.");
        Response = response;
    }
    
    public void SetResponse(BinaryData data, Type responseType)
    {
        if (Response is not null)
            throw new InvalidOperationException("Response has already been set.");

        ResponseData = data;
        
        Response = 
            JsonSerializer.Deserialize(data, responseType)
            ?? throw new InvalidOperationException($"Failed to deserialize message body to type '{responseType.Name}'.");
    }

    public void SetResponseError(string? error)
    {
        ResponseError = error;
    }
    
    public void SetCommandData(BinaryData data) => CommandData = data;
    public void SetResponseData(BinaryData data) => ResponseData = data;
    
    public void SetCommandRepository(ICommandRepository repository)
    {
        if (_commandRepository is not null)
            throw new InvalidOperationException("Command repository has already been set.");

        _commandRepository = repository;
    }
    
    public IDictionary<string, object> ToDictionary() => new Dictionary<string, object>
    {
        { "CorrelationId", CorrelationId },
        { "SendingServiceId", SendingServiceId },
        { "CommandNamespace", CommandNamespace }
    };
}
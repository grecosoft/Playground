using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Common.Messaging.Entities;

namespace Common.Messaging.Commands;

public class CommandContext(
    string correlationId,
    string sendingServiceId,
    string commandNamespace)
{
    private BinaryData? _commandData;
    private ICommandMessage? _command;
    
    private BinaryData? _responseData;

    private ICommandRepository? _commandRepository;

    public string CorrelationId { get; } = correlationId;
    public string SendingServiceId { get; } = sendingServiceId;
    public string CommandNamespace { get; } = commandNamespace;

    public ICommandMessage Command => _command ?? throw new InvalidOperationException("Command not set.");
    public BinaryData CommandData => _commandData ?? throw new InvalidOperationException("Command data not set.");
    public BinaryData ResponseData => _responseData ?? throw new InvalidOperationException("Response data not set.");
    
    public ICommandRepository CommandRepository => _commandRepository
                                                   ?? throw new InvalidOperationException("CommandRepository not set.");

    public Type CommandType => Command.GetType();
    public object? Response { get; private set; }

    public static CommandContext Create(ServiceBusReceivedMessage message)
    {
        return new CommandContext(
            message.CorrelationId,
            message.GetRequiredStringProperty(MessageProperties.SendingServiceId),
            message.GetRequiredStringProperty(MessageProperties.CommandNamespace));
    }

    public void SetCommand(BinaryData data, Type commandType)
    {
        if (_command is not null)
            throw new InvalidOperationException("Command has already been set.");

        _commandData = data;
        
        _command = (ICommandMessage)(
            JsonSerializer.Deserialize(data, commandType)
            ?? throw new InvalidOperationException($"Failed to deserialize message body to type '{commandType.Name}'.")
        );
    }
    
    public void SetResponse(BinaryData data, Type responseType)
    {
        if (Response is not null)
            throw new InvalidOperationException("Response has already been set.");

        _responseData = data;
        
        Response = 
            JsonSerializer.Deserialize(data, responseType)
            ?? throw new InvalidOperationException($"Failed to deserialize message body to type '{responseType.Name}'.");
    }
    
    public void SetResponse(BinaryData data)
    {
        if (_responseData is not null)
            throw new InvalidOperationException("ResponseData has already been set.");

        _responseData = data;
    }
    
    

    public void SetCommandData(BinaryData data)
    {
        if (_commandData is not null)
        {
            throw new InvalidOperationException("Command has already been set.");
        }
        
        _commandData = data;
    }

    public void SetCommandRepository(ICommandRepository repository)
    {
        if (_commandRepository is not null)
            throw new InvalidOperationException("Command repository has already been set.");

        _commandRepository = repository;
    }

    public void SetResponse(object response)
    {
        if (Response is not null)
            throw new InvalidOperationException("Response has already been set.");
        Response = response;
    }

    public IDictionary<string, object> ToDictionary() => new Dictionary<string, object>
    {
        { "CorrelationId", CorrelationId },
        { "SendingServiceId", SendingServiceId },
        { "CommandNamespace", CommandNamespace }
    };
}
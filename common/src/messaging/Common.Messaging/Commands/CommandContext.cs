using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Common.Messaging.Entities;

namespace Common.Messaging.Commands;

public class CommandContext(
    string correlationId,
    string sendingServiceId,
    string commandNamespace)
{
    private ICommandMessage? _command;
    private ICommandRepository? _commandRepository;

    public string CorrelationId { get; } = correlationId;
    public string SendingServiceId { get; } = sendingServiceId;
    public string CommandNamespace { get; } = commandNamespace;

    public ICommandMessage Command => _command ?? throw new InvalidOperationException("Command not set.");

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

        _command = (ICommandMessage)(
            JsonSerializer.Deserialize(data, commandType)
            ?? throw new InvalidOperationException($"Failed to deserialize message body to type '{commandType.Name}'.")
        );
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
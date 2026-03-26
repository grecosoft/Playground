using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Playground.Common.Messaging.Types;

namespace Playground.Common.Messaging.Services;

public class ReceivedCommand(
    string correlationId,
    string replyToServiceId,
    string commandNamespace,
    DispatchStrategyType dispatchStrategy)
{
    private ICommandMessage? _command;
    private Type? _commandType;

    public string CorrelationId { get; } = correlationId;
    public string ReplyToServiceId { get; } = replyToServiceId;
    public string CommandNamespace { get; } = commandNamespace;
    public DispatchStrategyType DispatchStrategy { get; } = dispatchStrategy;
    
    public ICommandMessage Command => _command ?? throw new InvalidOperationException("Command not set.");
    public Type CommandType => _commandType ?? throw new InvalidOperationException("CommandType not set.");

    public static ReceivedCommand Create(ServiceBusReceivedMessage message)
    {
        return new ReceivedCommand(
            message.CorrelationId,
            message.GetRequiredStringProperty(MessageProperties.SendingServiceId),
            message.GetRequiredStringProperty(MessageProperties.CommandNamespace),
            message.GetRequiredEnumProperty<DispatchStrategyType>(MessageProperties.DispatchStrategyType));
    }

    public void SetCommand(ICommandMessage command)
    {
        _command = command;
        _commandType = command.GetType();
    }
    
    public void SetCommand(ServiceBusReceivedMessage message, Type commandType)
    {
        _command = (ICommandMessage)(
            JsonSerializer.Deserialize(message.Body, commandType)
            ?? throw new InvalidOperationException() // todo
        );
        
        _commandType = commandType;
    }
}
using System.Text.Json;
using Playground.Common.Messaging.Types;

namespace Playground.Common.Messaging.Services;

public class CommandRepository : ICommandRepository
{
    private readonly Dictionary<string, CommandState> _commandStates = new();
    
    public Task SaveCommand(ReceivedCommand receivedCommand, CancellationToken cancellationToken)
    {
        var commandState = new CommandState(
            receivedCommand.CorrelationId,
            receivedCommand.ReplyToService,
            receivedCommand.CommandNamespace,
            JsonSerializer.Serialize(receivedCommand.Command, receivedCommand.CommandType));

        _commandStates[receivedCommand.CorrelationId] = commandState;
        
        return Task.CompletedTask;
    }

    public Task<ReceivedCommand> LoadCommand<T>(string correlationId, CancellationToken cancellationToken)
        where T : ICommandMessage
    {
        if (!_commandStates.TryGetValue(correlationId, out var commandState))
        {
            throw new KeyNotFoundException($"Command {correlationId} not found.");
        }

        var receivedCommand = new ReceivedCommand(
            commandState.CorrelationId,
            commandState.ReplyToService,
            commandState.CommandNamespace, 
            DispatchStrategyType.Async);
        
        receivedCommand.SetCommand(
            JsonSerializer.Deserialize<T>(commandState.command)
            ?? throw new InvalidOperationException($"Command {commandState.command} not deserialized."));
        
        
        return Task.FromResult(receivedCommand);
    }

    private record CommandState(
        string CorrelationId, 
        string ReplyToService,
        string CommandNamespace,
        string command);
}
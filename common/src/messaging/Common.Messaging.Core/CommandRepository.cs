using System.Text.Json;
using Common.Messaging.Entities;

namespace Common.Messaging.Core;

public class CommandRepository : ICommandRepository
{
    private readonly Dictionary<string, CommandState> _commandStates = new();
    
    public Task SaveCommand(ReceivedCommand receivedCommand, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Command {receivedCommand.CorrelationId} saved.");
        
        var commandState = new CommandState(
            receivedCommand.CorrelationId,
            receivedCommand.ReplyToServiceId,
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
            commandState.CommandNamespace);
        
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
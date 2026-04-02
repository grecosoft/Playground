using System.Text.Json;
using Common.Messaging.Commands;
using Common.Messaging.Entities;

namespace Common.Messaging.Core.Commands;

public class CommandRepository : ICommandRepository
{
    private readonly Dictionary<string, CommandState> _commandStates = new();
    
    public Task SaveCommand(CommandContext commandContext, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Command {commandContext.CorrelationId} saved.");
        
        var commandState = new CommandState(
            commandContext.CorrelationId,
            commandContext.SendingServiceId,
            commandContext.CommandNamespace,
            JsonSerializer.Serialize(commandContext.Command, commandContext.CommandType));

        _commandStates[commandContext.CorrelationId] = commandState;
        
        return Task.CompletedTask;
    }

    public Task<CommandContext> LoadCommand<T>(string correlationId, CancellationToken cancellationToken)
        where T : ICommandMessage
    {
        if (!_commandStates.TryGetValue(correlationId, out var commandState))
        {
            throw new KeyNotFoundException($"Command {correlationId} not found.");
        }

        var receivedCommand = new CommandContext(
            commandState.CorrelationId,
            commandState.ReplyToService,
            commandState.CommandNamespace);
        
        receivedCommand.SetCommand(BinaryData.FromString(commandState.command), typeof(T));
        
        return Task.FromResult(receivedCommand);
    }

    private record CommandState(
        string CorrelationId, 
        string ReplyToService,
        string CommandNamespace,
        string command);
}
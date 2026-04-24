using System.Text.Json;
using Common.Messaging.Commands;
using Common.Messaging.Entities;

namespace Common.Messaging.Core.Commands;

/// <summary>
/// NOTE:  This is a simple in-memory implementation of the command repository for demonstration purposes.
/// In a production system, you would likely want to implement this using a durable storage mechanism such as a database.
/// </summary>
public class CommandRepository : ICommandRepository
{
    private readonly Dictionary<string, CommandState> _commandStates = new();
    
    public Task<Guid> SaveCommandContext(CommandContext commandContext, CancellationToken ct)
    {
        Console.WriteLine($"Command {commandContext.CorrelationId} saved.");
        
        var commandState = new CommandState(
            commandContext.CorrelationId,
            commandContext.SendingServiceId,
            commandContext.SendingServiceName,
            commandContext.CommandNamespace,
            JsonSerializer.Serialize(commandContext.Command, commandContext.CommandType));

        _commandStates[commandContext.CorrelationId] = commandState;
        
        return Task.FromResult(Guid.NewGuid());
    }

    public Task<CommandContext> LoadTypedCommandContext<T>(string correlationId, CancellationToken ct)
        where T : ICommandMessage
    {
        if (!_commandStates.TryGetValue(correlationId, out var commandState))
        {
            throw new KeyNotFoundException($"Command {correlationId} not found.");
        }

        var receivedCommand = new CommandContext(
            commandState.CorrelationId,
            commandState.ReplyToServiceId,
            commandState.ReplyToServiceName,
            commandState.CommandNamespace);
        
        receivedCommand.SetCommand(BinaryData.FromString(commandState.Command), typeof(T));
        
        return Task.FromResult(receivedCommand);
    }

    public Task<CommandContext> LoadCommandContext(string correlationId, CancellationToken ct)
    {
        if (!_commandStates.TryGetValue(correlationId, out var commandState))
        {
            throw new KeyNotFoundException($"Command {correlationId} not found.");
        }

        var receivedCommand = new CommandContext(
            commandState.CorrelationId,
            commandState.ReplyToServiceId,
            commandState.ReplyToServiceName,
            commandState.CommandNamespace);
        
        receivedCommand.SetCommandData(BinaryData.FromString(commandState.Command));
        
        return Task.FromResult(receivedCommand);
    }

    public Task DeleteCommandCommand(string correlationId, CancellationToken ct)
    {
        _commandStates.Remove(correlationId);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<CommandContext>> GetPendingCommandContextsAsync(CancellationToken ct)
    {
        var pendingCommands = _commandStates.Values.Select(s => new CommandContext(
            s.CorrelationId,
            s.ReplyToServiceId,
            s.ReplyToServiceName,
            s.CommandNamespace));
        
        return Task.FromResult(pendingCommands);
    }

    private record CommandState(
        string CorrelationId, 
        string ReplyToServiceId,
        string ReplyToServiceName,
        string CommandNamespace,
        string Command);
}
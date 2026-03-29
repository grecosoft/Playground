using Playground.Common.Messaging.Services;

namespace Playground.Common.Messaging;

public class CommandContext(
    ReceivedCommand receivedCommand)
{
    public ReceivedCommand ReceivedCommand { get; } = receivedCommand;
    
    public object? Response { get; protected set; }
}

public class CommandContext<TResponse>(
    ReceivedCommand receivedCommand,
    ICommandRepository commandRepository) :
    CommandContext(receivedCommand)
{
    public void SetResponse(TResponse response)
    {
        Response = response;
    }

    public Task SaveCommandAsync(CancellationToken cancellationToken)
    {
        return commandRepository.SaveCommand(ReceivedCommand, cancellationToken);
    }
}
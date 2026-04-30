using System.Text.Json;
using Common.Messaging.Commands;
using Messaging.Demo.Common.Commands;

namespace Messaging.Demo.One.Api.Handlers;

public class ConnectorStatusHandler(
    ILogger<ConnectorStatusHandler> logger) 
    : CommandHandlerBase<ConnectorStatusCommand, ConnectorStatusResponse>
{
    protected override Task HandleMessage(ConnectorStatusCommand command,
        CommandContext context,
        CancellationToken cancellationToken)
    {
        logger.LogWarning("Response Message: {message}", JsonSerializer.Serialize(command));
        return Task.CompletedTask;
    }
}
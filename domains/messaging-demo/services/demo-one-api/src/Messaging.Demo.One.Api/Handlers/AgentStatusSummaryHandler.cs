using System.Text.Json;
using Common.Messaging.Commands;
using Messaging.Demo.Common.Commands;

namespace Messaging.Demo.One.Api.Handlers;

public class AgentStatusSummaryHandler(
    ILogger<AgentStatusSummaryHandler> logger) 
    : CommandHandlerBase<AgentStatusCommand, AgentStatusSummaryResponse>
{
    protected override Task HandleMessage(AgentStatusCommand command,
        CommandContext context,
        CancellationToken cancellationToken)
    {
        logger.LogWarning("Response Message: {message}", JsonSerializer.Serialize(command));
        return Task.CompletedTask;
    }
}
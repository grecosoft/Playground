using System.Text.Json;
using Common.Messaging.Commands;
using Messaging.Demo.Common.Commands;
using Serilog;

namespace Messaging.Demo.One.Api.Handlers;

public class AgentStatusSummaryHandler() 
    : CommandHandlerBase<AgentStatusSummaryCommand, AgentStatusSummaryResponse>
{
    protected override Task HandleMessage(AgentStatusSummaryCommand command,
        CommandContext context,
        CancellationToken cancellationToken)
    {
        Log.Logger.Warning("Response Message: {message}", JsonSerializer.Serialize(command));
        return Task.CompletedTask;
    }
}
using System.Text.Json;
using Common.Messaging.Commands;
using Messaging.Demo.Common.Commands;

namespace Messaging.Demo.One.Api.Handlers;

public class AgentStatusSummaryHandler : CommandHandlerBase<AgentStatusSummaryCommand, AgentStatusSummaryResponse>
{
    protected override Task HandleMessage(AgentStatusSummaryCommand command,
        CommandContext context,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"{nameof(DeviceUpdateCommand)} received: {JsonSerializer.Serialize(command)}");
        
        return Task.CompletedTask;
    }
}
using Common.Messaging.Commands;

namespace Messaging.Hub.Api.Models;

public class PendingCommandModel(CommandContext context)
{
    public string CorrelationId => context.CorrelationId;
    public string SendingServiceId => context.SendingServiceId;
    public string CommandNamespace => context.CommandNamespace;
}
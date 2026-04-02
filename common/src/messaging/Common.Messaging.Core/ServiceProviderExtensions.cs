using Common.Messaging.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Messaging.Core;

public static class ServiceProviderExtensions
{
    public static ICommandMessageHandler GetCommandHandler(
        this IServiceProvider serviceProvider,
        CommandContext commandContext)
    {
        var handler = serviceProvider.GetKeyedService<ICommandMessageHandler>(
            commandContext.CommandNamespace);

        return handler ?? throw new InvalidOperationException("");
    }
}
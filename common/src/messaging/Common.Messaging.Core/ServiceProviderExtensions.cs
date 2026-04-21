using Common.Messaging.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Messaging.Core;

public static class ServiceProviderExtensions
{
    /// <summary>
    /// Returns the command handler for the specified command context, which includes the
    /// command namespace used to resolve the handler from the service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="commandContext">The context of the command.</param>
    /// <returns>Command handler or exception if not found.</returns>
    public static ICommandMessageHandler GetCommandHandler(
        this IServiceProvider serviceProvider,
        CommandContext commandContext)
    {
        // NOTE: When the AddBusMessaging extension method is called when bootstrapping the service
        // handlers are registered with the command namespace as the key.
        var handler = serviceProvider.GetKeyedService<ICommandMessageHandler>(
            commandContext.CommandNamespace);

        return handler ?? throw new InvalidOperationException(
            $"Handler for command {commandContext.CommandNamespace} not registered.");
    }
}
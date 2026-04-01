using Microsoft.Extensions.DependencyInjection;

namespace Common.Messaging.Core;

public static class ServiceProviderExtensions
{
    public static ICommandMessageHandler GetCommandHandler(
        this IServiceProvider serviceProvider,
        ReceivedCommand receivedCommand)
    {
        var handler = serviceProvider.GetKeyedService<ICommandMessageHandler>(
            receivedCommand.CommandNamespace);

        return handler ?? throw new InvalidOperationException("");
    }
}
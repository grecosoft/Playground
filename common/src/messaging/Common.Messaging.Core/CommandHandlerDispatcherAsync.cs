using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Common.Messaging.Core;

public class CommandHandlerDispatcherAsync(
    ILogger<CommandHandlerDispatcherAsync> logger,
    IServiceProvider serviceProvider,
    [FromKeyedServices("async")]ServiceBusProcessor requestTopicProcessor)
{
    
}
using Azure.Messaging.ServiceBus;

namespace Common.Messaging;

public static class MessageProperties
{
    /// <summary>
    /// Message property storing unique key used to identify the sending service used
    /// to route the message when replying to a command.
    /// </summary>
    public const string SendingServiceId = "sending-service-id";
    
    /// <summary>
    /// Key name used to reference service by name in code and also used within log messages.
    /// </summary>
    public const string SendingServiceName = "sending-service-name";
    
    /// <summary>
    /// The identity of the service to which the command should be delivered.
    /// </summary>
    public const string EndpointServiceId = "service_id";
    
    /// <summary>
    /// Message property used to uniquely identify the message within a solution of services.
    /// </summary>
    public const string CommandNamespace = "command-namespace";
    
    /// <summary>
    /// Extension methods for retrieving message properties. 
    /// </summary>
    /// <param name="message">The message containing properties to be retrieved.</param>
    extension(ServiceBusReceivedMessage message)
    {
        public string GetRequiredStringProperty(string key)
        {
            if (!message.ApplicationProperties.TryGetValue(key, out var value) 
                || string.IsNullOrWhiteSpace(value?.ToString()))
            {
                throw new ArgumentException(
                    $"Received message is missing required application property '{key}'.",
                    nameof(key));
            }

            return value.ToString()!;
        }
    }
}
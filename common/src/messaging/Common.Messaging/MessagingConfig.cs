namespace Common.Messaging;

/// <summary>
/// Configuration settings for messaging between services within a solution.
/// All the needed values are set by the service_messaging Terraform module.
/// </summary>
public class MessagingConfig
{
    /// <summary>
    /// The unique identifier of the service within the solution, used for routing messages to the correct service.
    /// </summary>
    public string ServiceId { get; set; } = string.Empty;
    
    /// <summary>
    /// The name of the solution in which the service exists.
    /// </summary>
    public string SolutionName { get; set; } = string.Empty;
    
    /// <summary>
    /// The name of the service.
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;
    
    /// <summary>
    /// The Azure Service Bus namespace that the solution's services use to communicate with each other.
    /// </summary>
    public string ServiceBusHostName { get; set; } = string.Empty;
    
    /// <summary>
    /// A dictionary of dependent services that this service needs to communicate with,
    /// where the key is the name of the service and the value is its identity.
    /// </summary>
    public Dictionary<string, string> DependentServices { get; init; } = [];

    /// <summary>
    /// The number of seconds to wait for a response to an RPC style of command.
    /// </summary>
    public int RpcReplyTimeoutSeconds { get; set; } = 50;
    
    
    
    /// <summary>
    /// The name of the topic used to send RPC style commands between a solution's services.
    /// </summary>
    public string RpcCommandTopic => $"{SolutionName}-command-rpc-topic";

    /// <summary>
    /// The name of the subscription used to receive RPC style commands for this service.    
    /// </summary>
    public string RpcCommandSubscription => $"{ServiceName}-rpc-commands";
    
    /// <summary>
    /// The name of the reply queue used to send responses to received RPC style commands back
    /// to the originating solution's service.  
    /// </summary>
    public string RpcReplyQueue => $"{SolutionName}-command-rpc-reply-queue";
    
    /// <summary>
    /// The name of the topic used to send asynchronous commands between a solution's services,
    /// where the sender does not expect an immediate response from the receiving service.
    /// </summary>
    public string AsyncCommandTopic => $"{SolutionName}-command-async-topic";

    /// <summary>
    /// The name of the subscription used to receive asynchronous commands for this service.
    /// </summary>
    public string AsyncCommandSubscription => $"{ServiceName}-async-commands";

    public object ToLoggableProperties() => new
    {
        ServiceId,
        ServiceBusHostName,
        RpcCommandTopic,
        RpcCommandSubscription,
        RpcReplyQueue,
        RpcReplyTimeoutSeconds
    };
}
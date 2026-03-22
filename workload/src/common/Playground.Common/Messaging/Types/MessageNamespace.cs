namespace Playground.Common.Messaging.Types;

[AttributeUsage(AttributeTargets.Class)]
public class MessageNamespace(
    string serviceName, 
    string namespaceName,
    DispatchStrategyType commandType) : Attribute
{
    public string ServiceName { get; } = serviceName;
    public string NamespaceName { get; } = namespaceName;
    public DispatchStrategyType CommandType { get; } = commandType;
}
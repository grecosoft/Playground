namespace Playground.Common.Messaging.Types;

[AttributeUsage(AttributeTargets.Class)]
public class MessageNamespace(string serviceName, string namespaceName) : Attribute
{
    public string ServiceName { get; } = serviceName;
    public string NamespaceName { get; } = namespaceName;
}
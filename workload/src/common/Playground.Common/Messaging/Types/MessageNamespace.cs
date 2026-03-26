namespace Playground.Common.Messaging.Types;

[AttributeUsage(AttributeTargets.Class)]
public class MessageNamespace(
    string namespaceName) : Attribute
{
    public string NamespaceName { get; } = namespaceName;
}
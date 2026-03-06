namespace Playground.Common.Messaging.Types;

[AttributeUsage(AttributeTargets.Class)]
public class CommandNamespace(string namespaceName) : Attribute
{
    public string NamespaceName { get; } = namespaceName;
}
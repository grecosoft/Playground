namespace Common.Messaging.Entities;

/// <summary>
/// Attribute used to specify the namespace of messages handled by a message handler class.
/// Once a command namespace is set for a command, it should not be changed as it is used to
/// route messages to the correct handlers.
/// </summary>
/// <param name="namespaceName">Period delimited value containing a hierarchical value identify the command.</param>
[AttributeUsage(AttributeTargets.Class)]
public class MessageNamespace(
    string namespaceName) : Attribute
{
    public string NamespaceName { get; } = namespaceName;
}
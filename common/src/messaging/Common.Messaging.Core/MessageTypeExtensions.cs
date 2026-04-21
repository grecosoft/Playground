using System.Reflection;
using Common.Messaging.Commands;
using Common.Messaging.Entities;

namespace Common.Messaging.Core;

public record CommandDispatchInfo(
    string CommandNamespace,
    Type ImplementationType);

public static class MessageTypeExtensions
{
    /// <summary>
    /// Scans a list of types searching for concrete classes responsible for handling commands.
    /// TODO:  Add validation logic to validate there is only one handler per command type.
    /// </summary>
    /// <param name="types">List of types to search.</param>
    /// <returns>Information used at runtime to dispatch a handler for a given command.</returns>
    public static IEnumerable<CommandDispatchInfo> GetCommandDispatches(this IEnumerable<TypeInfo> types)
    {
        return types.Where(t =>
                t is { IsClass: true, IsAbstract: false } &&
                t.ImplementedInterfaces.Contains(typeof(ICommandMessageHandler)))
            .SelectMany(GetCommandDispatches);
    }

    private static IEnumerable<CommandDispatchInfo> GetCommandDispatches(TypeInfo handlerType)
    {
        var handleMethod = handlerType.DeclaredMethods.FirstOrDefault(m =>
            m is { IsFamily: true, Name: "HandleMessage" } &&
            m.GetParameters() is { Length: > 0 } parameters &&
            parameters[0].ParameterType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(ICommandMessage)));

        if (handleMethod is null) return [];

        var commandType = handleMethod.GetParameters()[0].ParameterType;
        var namespaceAttribute = commandType.GetCustomAttribute<MessageNamespace>();

        return namespaceAttribute is not null
            ? [new CommandDispatchInfo(namespaceAttribute.NamespaceName, handlerType)]
            : [];
    }
}
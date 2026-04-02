using System.Reflection;
using Common.Messaging.Commands;
using Common.Messaging.Entities;

namespace Common.Messaging.Core;

public record CommandDispatchInfo(
    string CommandNamespace,
    Type ImplementationType);

public static class MessageTypeExtensions
{
    public static IEnumerable<CommandDispatchInfo> GetCommandDispatches(this IEnumerable<TypeInfo> types)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.DefinedTypes)
            .Where(t =>
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
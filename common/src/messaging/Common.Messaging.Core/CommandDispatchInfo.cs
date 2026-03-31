namespace Common.Messaging.Core;

public record CommandDispatchInfo(
    string CommandNamespace,
    Type ImplementationType);
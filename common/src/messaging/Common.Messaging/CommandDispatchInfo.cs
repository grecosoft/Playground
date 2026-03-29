namespace Common.Messaging;

public record CommandDispatchInfo(
    string CommandNamespace,
    Type ImplementationType);
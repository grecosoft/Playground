namespace Playground.Common.Messaging;

public record CommandDispatchInfo(
    string CommandNamespace,
    Type ImplementationType);
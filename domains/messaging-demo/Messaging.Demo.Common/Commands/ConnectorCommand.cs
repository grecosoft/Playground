using System.Text.Json.Serialization;

namespace Messaging.Demo.Common.Commands;

public record ConnectorCommand(
    [property: JsonPropertyName("connectorId")] string ConnectorId);
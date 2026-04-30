using System.Text.Json.Serialization;

namespace CSharp.Connector.Models;

public record CommandReplyResultModel(
    [property: JsonPropertyName("correlationId")]string CorrelationId, 
    [property: JsonPropertyName("commandNamespace")] string CommandNamespace,
    [property: JsonPropertyName("Successful")]bool Successful, 
    [property: JsonPropertyName("message")] string? Message,
    [property: JsonPropertyName("validations")]ValidationErrorModel? Validations);
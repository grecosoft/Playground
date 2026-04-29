using System.Text.Json.Serialization;

namespace Acme.Agent.Api.Models;

public record CommandReplyResultModel(
    [property: JsonPropertyName("correlationId")]string CorrelationId, 
    [property: JsonPropertyName("commandNamespace")] string CommandNamespace,
    [property: JsonPropertyName("Successful")]bool Successful, 
    [property: JsonPropertyName("message")] string? Message,
    [property: JsonPropertyName("validations")]ValidationErrorModel? Validations);
using System.Text.Json.Serialization;

namespace Acme.Agent.Api.Models;

public record ValidationErrorModel(
    [property: JsonPropertyName("errors")]IDictionary<string, string[]> Errors);
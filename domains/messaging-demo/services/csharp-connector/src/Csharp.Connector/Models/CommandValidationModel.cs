using System.Text.Json.Serialization;

namespace CSharp.Connector.Models;

public record ValidationErrorModel(
    [property: JsonPropertyName("errors")]IDictionary<string, string[]> Errors);
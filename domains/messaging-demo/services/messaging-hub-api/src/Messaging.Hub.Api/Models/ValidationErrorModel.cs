using System.Text.Json.Serialization;

namespace Messaging.Hub.Api.Models;

// TODO:  After cleanly extracting the JSON Schema errors update model:

/// <summary>
/// Model returned to the client when the command validation fails. It contains
/// the list of validation errors that occurred during the validation process.
/// </summary>
public record ValidationErrorModel(
    [property: JsonPropertyName("errors")]IDictionary<string, string> Errors);
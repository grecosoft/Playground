using System.Text.Json.Serialization;

namespace Messaging.Hub.Api.Models;

public record CommandReplyResultModel(
    [property: JsonPropertyName("correlationId")] string CorrelationId, 
    [property: JsonPropertyName("commandNamespace")] string CommandNamespace,
    [property: JsonPropertyName("Successful")]bool Successful, 
    [property: JsonPropertyName("message")] string? Message = null,
    [property: JsonPropertyName("validations")] ValidationErrorModel? Validations = null)
{ 
    public static CommandReplyResultModel Success(string correlationId, string commandNamespace) =>
        new (correlationId, commandNamespace, true);
   
    public static CommandReplyResultModel Failed(
        string correlationId, 
        string commandNamespace,
        string message) => new (correlationId, commandNamespace, false, message);
    
    public static CommandReplyResultModel Failed(
        string correlationId, 
        string commandNamespace,
        string message,
        ValidationErrorModel validations) => new (correlationId, commandNamespace, false, message, validations);
}
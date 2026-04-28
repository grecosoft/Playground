namespace Messaging.Hub.Domain.Entities;

public record ValidationResult(
    string CommandNameSpace,
    bool IsValid,
    string ErrorMessage,
    IDictionary<string, string> Errors)
{

    public static ValidationResult Valid(string commandNamespace) =>
        new(commandNamespace, true, string.Empty, new Dictionary<string, string>());
    
    public static ValidationResult Invalid(string commandNamespace, string errorMessage) =>
        new(commandNamespace, false, errorMessage, new Dictionary<string, string>());
    
    public static ValidationResult Invalid(
        string commandNamespace, 
        string errorMessage, 
        IDictionary<string, string> errors) => new(commandNamespace, false, errorMessage, errors);
}
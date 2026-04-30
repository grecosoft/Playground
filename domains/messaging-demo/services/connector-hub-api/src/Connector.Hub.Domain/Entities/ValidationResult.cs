namespace Connector.Hub.Domain.Entities;

public record ValidationResult(
    string CommandNameSpace,
    bool IsValid,
    string ErrorMessage,
    IDictionary<string, IList<string>> Errors)
{

    public static ValidationResult Valid(string commandNamespace) =>
        new(commandNamespace, true, string.Empty, new Dictionary<string, IList<string>>());
    
    public static ValidationResult Invalid(string commandNamespace, string errorMessage) =>
        new(commandNamespace, false, errorMessage, new Dictionary<string, IList<string>>());
    
    public static ValidationResult Invalid(
        string commandNamespace, 
        string errorMessage, 
        IDictionary<string, IList<string>> errors) => new(commandNamespace, false, errorMessage, errors);
}
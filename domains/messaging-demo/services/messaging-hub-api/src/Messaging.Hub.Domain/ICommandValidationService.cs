using Messaging.Hub.Domain.Entities;

namespace Messaging.Hub.Domain;

public record CommandSchema(
    string CommandNamespace, 
    string CommandName);

public interface ICommandValidationService
{
    ValidationResult ValidateCommand(string commandNamespace, string json);
    ValidationResult ValidateResponse(string commandNamespace, string json);
}
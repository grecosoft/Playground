using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Pointer;
using Json.Schema;
using Connector.Hub.Domain;
using Connector.Hub.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Connector.Hub.Infra;

public class CommandValidationService(
    string schemaDirectory) : ICommandValidationService
{
    private readonly Dictionary<string, SchemaDefinition> _schemaDefinitions = [];

    public void LoadSchemas(ILogger logger, CommandSchema[] commandSchemas)
    {
        foreach (var commandSchema in commandSchemas)
        {
            var jsonSchema = ReadSchema(logger, commandSchema);
            if (jsonSchema is null)
            {
                continue;
            }
            
            var commandDef = GetDefinitionSchema(logger, $"{commandSchema.CommandName}Command", jsonSchema);
            var responseDef = GetDefinitionSchema(logger, $"{commandSchema.CommandName}Response", jsonSchema);
            
            if (commandDef is null || responseDef is null)
            {
                continue;
            }
            
            _schemaDefinitions[commandSchema.CommandNamespace] = new SchemaDefinition(
                commandSchema.CommandNamespace,
                commandSchema.CommandName,
                commandDef,
                responseDef);
        }
    }
    
    public ValidationResult ValidateCommand(string commandNamespace, string json)
    {
        if (!_schemaDefinitions.TryGetValue(commandNamespace, out var schemaDefinition))
        {
            return ValidationResult.Invalid(json, $"{commandNamespace} is not a valid command namespace.");
        }

        var results = schemaDefinition.CommandSchema.Evaluate(
            JsonDocument.Parse(json).RootElement,
            DefaultOptions);
        
        if (results.IsValid)
        {
            return ValidationResult.Valid(commandNamespace);
        }
    
        return ValidationResult.Invalid(
            commandNamespace, 
            "Response validation failed.",
            results.ToErrorList());
    }

    public ValidationResult ValidateResponse(string commandNamespace, string json)
    {
        if (!_schemaDefinitions.TryGetValue(commandNamespace, out var schemaDefinition))
        {
            return ValidationResult.Invalid(json, $"{commandNamespace} is not a valid command namespace.");
        }
        
        var results = schemaDefinition.ResponseSchema.Evaluate(
            JsonDocument.Parse(json).RootElement,
            DefaultOptions);
        
        if (results.IsValid)
        {
            return ValidationResult.Valid(commandNamespace);
        }
        
        return ValidationResult.Invalid(
            commandNamespace, 
            "Response validation failed.",
            results.ToErrorList());
    }
    
    private static EvaluationOptions DefaultOptions => new EvaluationOptions {  OutputFormat = OutputFormat.List };
    
    private JsonSchema? GetDefinitionSchema(
        ILogger logger,
        string definitionName,
        JsonNode jsonSchema)
    {
        if (!ValidateDefinitionExists(jsonSchema, definitionName))
        {
            logger.LogError("Schema definition file could not be found: {schemaPath}", definitionName);
            return null;
        }
        
        var definitionSchema = JsonNode.Parse(jsonSchema.ToJsonString())!;
        definitionSchema["$ref"] = $"#/$defs/{definitionName}";
        
        return JsonSchema.Build(definitionSchema.Deserialize<JsonElement>());
    }

    private JsonNode? ReadSchema(ILogger logger, CommandSchema commandSchema)
    {
        var schemaPath = Path.Combine(schemaDirectory, $"{commandSchema.CommandNamespace}.json");
        if (!File.Exists(schemaPath)) 
        {
            logger.LogError("Schema definition file could not be found: {schemaPath}", schemaPath);
        }
        
        var schemaJson = JsonNode.Parse(File.ReadAllText(schemaPath));
        if (schemaJson is null)
        {
            logger.LogError("Schema definition file could not be parsed: {schemaPath}", schemaPath);
        }
        
        return schemaJson;
    }

    private bool ValidateDefinitionExists(JsonNode schema, string definitionName)
    {
        var pointer = JsonPointer.Parse($"/$defs/{definitionName}");
        return pointer.TryEvaluate(schema, out _);
    }
}

public record SchemaDefinition(
    string CommandNamespace,
    string CommandName,
    JsonSchema CommandSchema,
    JsonSchema ResponseSchema);
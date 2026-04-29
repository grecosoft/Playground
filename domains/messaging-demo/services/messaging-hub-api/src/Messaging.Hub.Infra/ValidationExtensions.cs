using Json.Schema;

namespace Messaging.Hub.Infra;

public static class ValidationExtensions
{
    public static IDictionary<string, IList<string>> ToErrorList(this EvaluationResults evaluationResults)
    {
        IDictionary<string, IList<string>> errors = new Dictionary<string, IList<string>>();

        foreach (var detail in evaluationResults.Details ?? [])
        {
            AddErrors(detail, errors);
        }
    
        return errors;
    }
    
    private static void AddErrors(EvaluationResults detail, IDictionary<string, IList<string>> errors)
    {
        if (detail.Errors is { Count: > 0 })
        {
            foreach (var error in detail.Errors)
            {
                var propLocation = detail.InstanceLocation.ToString();
                if (errors.TryGetValue(propLocation, out var propErrors)) {
                    propErrors.Add(error.Value);
                    continue;
                }
      
                errors[detail.InstanceLocation.ToString()] = [error.Value];
            }
        }

        foreach (var evaluationResult in detail.Details ?? [])
        {
            AddErrors(evaluationResult, errors);
        }
    }
}
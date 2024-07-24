using FluentResults;
using FluentValidation.Results;

namespace Fei.Is.Api.Common.Errors;

public class ValidationError : Error
{
    public IDictionary<string, string[]> ValidationErrors { get; }

    public ValidationError(Dictionary<string, string[]> errors)
        : base("Validation error")
    {
        ValidationErrors = errors;
    }

    public ValidationError(string key, string message)
        : base("Validation error")
    {
        ValidationErrors = new Dictionary<string, string[]>
        {
            { key, new[] { message } }
        };
    }

    public ValidationError(ValidationResult validationResult)
        : base("Validation error")
    {
        ValidationErrors = validationResult.ToDictionary();
    }
}

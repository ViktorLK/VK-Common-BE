using VK.Blocks.Validation.Abstractions;
using VK.Blocks.Validation.Abstractions.Contracts;

namespace VK.Blocks.Validation.Pipeline;

/// <summary>
/// realization of <see cref="IValidationPipeline"/> that executes all registered validators.
/// </summary>
internal sealed class ValidationPipeline(IEnumerable<IValidator> validators) : IValidationPipeline
{
    public async Task<ValidationResult> ValidateAsync(object model, CancellationToken ct = default)
    {
        if (model == null)
        {
            return ValidationResult.Success();
        }

        var errors = new List<ValidationError>();

        foreach (var validator in validators)
        {
            if (validator.CanValidate(model))
            {
                var result = await validator.ValidateAsync(model, ct);
                if (!result.IsValid)
                {
                    errors.AddRange(result.Errors);
                }
            }
        }

        return errors.Count == 0 
            ? ValidationResult.Success() 
            : ValidationResult.Failure(errors);
    }
}

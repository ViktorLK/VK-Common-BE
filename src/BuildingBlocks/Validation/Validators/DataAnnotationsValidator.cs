using System.ComponentModel.DataAnnotations;
using VK.Blocks.Validation.Abstractions;
using VK.Blocks.Validation.Abstractions.Contracts;

namespace VK.Blocks.Validation.Validators;

/// <summary>
/// realization of <see cref="IValidator"/> that uses Data Annotations.
/// </summary>
internal sealed class DataAnnotationsValidator : IValidator
{
    public bool CanValidate(object model) => model != null;

    public Task<VK.Blocks.Validation.Abstractions.Contracts.ValidationResult> ValidateAsync(object model, CancellationToken ct = default)
    {
        var validationContext = new ValidationContext(model);
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

        bool isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);

        if (isValid)
        {
            return Task.FromResult(VK.Blocks.Validation.Abstractions.Contracts.ValidationResult.Success());
        }

        var errors = validationResults.Select(r => new ValidationError(
            r.MemberNames.FirstOrDefault() ?? string.Empty,
            r.ErrorMessage ?? "Value is invalid"
        ));

        return Task.FromResult(VK.Blocks.Validation.Abstractions.Contracts.ValidationResult.Failure(errors));
    }
}

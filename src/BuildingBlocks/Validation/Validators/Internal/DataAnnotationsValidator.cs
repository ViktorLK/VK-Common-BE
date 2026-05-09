using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Validation.Validators.Internal;

/// <summary>
/// realization of <see cref="IVKValidator"/> that uses Data Annotations.
/// </summary>
internal sealed class DataAnnotationsValidator : IVKValidator
{
    public bool CanValidate(object model)
    {
        VKGuard.NotNull(model);
        return true;
    }

    public Task<VKValidationResult> ValidateAsync(object model, CancellationToken ct = default)
    {
        VKGuard.NotNull(model);

        var validationContext = new ValidationContext(model);
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

        bool isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);

        if (isValid)
        {
            return Task.FromResult(VKValidationResult.Success());
        }

        var errors = validationResults.Select(r => new VKValidationError(
            r.MemberNames.FirstOrDefault() ?? string.Empty,
            r.ErrorMessage ?? "Value is invalid"
        ));

        return Task.FromResult(VKValidationResult.Failure(errors));
    }
}

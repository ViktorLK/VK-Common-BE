using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Validation.Abstractions;
using VK.Blocks.Validation.Abstractions.Contracts;

namespace VK.Blocks.Validation.Validators;

/// <summary>
/// realization of <see cref="VK.Blocks.Validation.Abstractions.IValidator"/> that wraps FluentValidation.
/// </summary>
internal sealed class FluentValidationValidator(IServiceProvider serviceProvider) : VK.Blocks.Validation.Abstractions.IValidator
{
    public bool CanValidate(object model)
    {
        if (model == null) return false;
        
        var validatorType = typeof(IValidator<>).MakeGenericType(model.GetType());
        return serviceProvider.GetService(validatorType) != null;
    }

    public async Task<ValidationResult> ValidateAsync(object model, CancellationToken ct = default)
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(model.GetType());
        var validator = (FluentValidation.IValidator?)serviceProvider.GetService(validatorType);

        if (validator == null)
        {
            return ValidationResult.Success();
        }

        var context = new ValidationContext<object>(model);
        var result = await validator.ValidateAsync(context, ct);

        if (result.IsValid)
        {
            return ValidationResult.Success();
        }

        var errors = result.Errors.Select(e => new ValidationError(
            e.PropertyName,
            e.ErrorMessage,
            e.ErrorCode
        ));

        return ValidationResult.Failure(errors);
    }
}

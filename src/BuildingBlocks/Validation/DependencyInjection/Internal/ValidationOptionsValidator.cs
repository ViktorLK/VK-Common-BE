using Microsoft.Extensions.Options;

namespace VK.Blocks.Validation.DependencyInjection.Internal;

/// <summary>
/// Validator for <see cref="VKValidationOptions"/>.
/// </summary>
internal sealed class ValidationOptionsValidator : IValidateOptions<VKValidationOptions>
{
    public ValidateOptionsResult Validate(string? name, VKValidationOptions options)
    {
        // The core validator now only checks core-level options.
        // Third-party providers (like FluentValidation) are managed via their own registration.
        return ValidateOptionsResult.Success;
    }
}

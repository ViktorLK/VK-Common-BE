using Microsoft.Extensions.Options;

namespace VK.Blocks.ExceptionHandling.DependencyInjection.Internal;

/// <summary>
/// Validator for <see cref="VKExceptionHandlingOptions"/>.
/// </summary>
internal sealed class ExceptionHandlingOptionsValidator : IValidateOptions<VKExceptionHandlingOptions>
{
    public ValidateOptionsResult Validate(string? name, VKExceptionHandlingOptions options)
    {
        // Add specific validation logic if needed
        return ValidateOptionsResult.Success;
    }
}

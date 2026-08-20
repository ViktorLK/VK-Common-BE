using Microsoft.Extensions.Options;

namespace VK.Blocks.Workflow;

/// <summary>
/// Validator for Workflow compensation options.
/// </summary>
public sealed class VKCompensationOptionsValidator : IValidateOptions<VKCompensationOptions>
{
    public ValidateOptionsResult Validate(string? name, VKCompensationOptions options)
    {
        if (options is null)
        {
            return ValidateOptionsResult.Fail("VKCompensationOptions instance cannot be null.");
        }

        if (options.MaxRetries < 0)
        {
            return ValidateOptionsResult.Fail("MaxRetries cannot be negative.");
        }

        if (options.RetryBaseDelayMs < 0)
        {
            return ValidateOptionsResult.Fail("RetryBaseDelayMs cannot be negative.");
        }

        return ValidateOptionsResult.Success;
    }
}

using Microsoft.Extensions.Options;

namespace VK.Blocks.Resilience.DependencyInjection.Internal;

internal sealed class ResilienceOptionsValidator : IValidateOptions<VKResilienceOptions>
{
    public ValidateOptionsResult Validate(string? name, VKResilienceOptions options)
    {
        if (options is null)
        {
            return ValidateOptionsResult.Fail("Options cannot be null.");
        }

        if (options.DefaultRetry.MaxRetries < 0)
        {
            return ValidateOptionsResult.Fail("MaxRetries must be non-negative.");
        }

        return ValidateOptionsResult.Success;
    }
}

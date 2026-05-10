using System;
using Microsoft.Extensions.Options;

namespace VK.Blocks.AI.DependencyInjection.Internal;

/// <summary>
/// Validator for <see cref="VKAIOptions"/>.
/// </summary>
internal sealed class AIOptionsValidator : IValidateOptions<VKAIOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, VKAIOptions options)
    {
        if (options == null)
        {
            return ValidateOptionsResult.Fail("AI options cannot be null.");
        }

        if (options.RetryCount < 0)
        {
            return ValidateOptionsResult.Fail("RetryCount must be greater than or equal to 0.");
        }

        if (options.Timeout <= TimeSpan.Zero)
        {
            return ValidateOptionsResult.Fail("Timeout must be greater than zero.");
        }

        if (options.CircuitBreakerThreshold <= 0)
        {
            return ValidateOptionsResult.Fail("CircuitBreakerThreshold must be greater than zero.");
        }

        return ValidateOptionsResult.Success;
    }
}

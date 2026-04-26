using Microsoft.Extensions.Options;

namespace VK.Blocks.Authentication.DependencyInjection.Internal;

/// <summary>
/// Validates the <see cref="VKAuthenticationOptions"/> during application startup.
/// </summary>
internal sealed class AuthenticationOptionsValidator : IValidateOptions<VKAuthenticationOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, VKAuthenticationOptions options)
    {
        // 0. Short-circuit if the entire block is disabled.
        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        // 1. Validate Base Options
        if (string.IsNullOrWhiteSpace(options.DefaultScheme))
        {
            return ValidateOptionsResult.Fail(VKAuthenticationConstants.DefaultSchemeRequired);
        }

        if (options.InMemoryCleanupIntervalMinutes < 1)
        {
            return ValidateOptionsResult.Fail(VKAuthenticationConstants.MinCleanupIntervalId);
        }

        return ValidateOptionsResult.Success;
    }
}

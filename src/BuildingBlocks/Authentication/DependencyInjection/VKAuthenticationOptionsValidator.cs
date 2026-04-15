using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Common;

namespace VK.Blocks.Authentication.DependencyInjection;

/// <summary>
/// Validates the <see cref="VKAuthenticationOptions"/> during application startup.
/// </summary>
public sealed class VKAuthenticationOptionsValidator : IValidateOptions<VKAuthenticationOptions>
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
            return ValidateOptionsResult.Fail(AuthenticationConstants.DefaultSchemeRequired);
        }

        if (options.InMemoryCleanupIntervalMinutes < 1)
        {
            return ValidateOptionsResult.Fail(AuthenticationConstants.MinCleanupIntervalId);
        }

        // 2. Validate Strategy Presence (Fail-Fast)
        if (options.Enabled && !options.Jwt.Enabled && !options.ApiKey.Enabled && !options.OAuth.Enabled)
        {
            return ValidateOptionsResult.Fail(AuthenticationConstants.AtLeastOneStrategyRequired);
        }

        return ValidateOptionsResult.Success;
    }
}

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

        // 2. Validate Strategy Consistency
        if (options.Jwt.Enabled && string.IsNullOrWhiteSpace(options.Jwt.SchemeName))
        {
            return ValidateOptionsResult.Fail(string.Format(VKAuthenticationConstants.JwtValidationFailedTemplate, options.Jwt.SchemeName));
        }

        if (options.OAuth.Enabled && string.IsNullOrWhiteSpace(VKOAuthOptions.SectionName))
        {
            return ValidateOptionsResult.Fail(string.Format(VKAuthenticationConstants.OAuthValidationFailedTemplate, VKOAuthOptions.SectionName));
        }

        if (options.ApiKey.Enabled && string.IsNullOrWhiteSpace(options.ApiKey.HeaderName))
        {
            return ValidateOptionsResult.Fail(string.Format(VKAuthenticationConstants.ApiKeyValidationFailedTemplate, options.ApiKey.HeaderName));
        }

        // 3. Overall validation (at least one strategy should be enabled if the block is enabled)
        if (!options.Jwt.Enabled && !options.ApiKey.Enabled && !options.OAuth.Enabled)
        {
            return ValidateOptionsResult.Fail(VKAuthenticationConstants.AtLeastOneStrategyRequired);
        }

        return ValidateOptionsResult.Success;
    }
}





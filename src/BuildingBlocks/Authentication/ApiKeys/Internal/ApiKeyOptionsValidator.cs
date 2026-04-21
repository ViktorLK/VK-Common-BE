using Microsoft.Extensions.Options;

namespace VK.Blocks.Authentication.ApiKeys.Internal;

/// <summary>
/// Validates the <see cref="VKApiKeyOptions"/> options.
/// </summary>
internal sealed class ApiKeyOptionsValidator : IValidateOptions<VKApiKeyOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, VKApiKeyOptions options)
    {
        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        if (string.IsNullOrWhiteSpace(options.HeaderName))
        {
            return ValidateOptionsResult.Fail("A valid HeaderName is required for API Key authentication.");
        }

        if (string.IsNullOrWhiteSpace(options.SchemeName))
        {
            return ValidateOptionsResult.Fail("A valid SchemeName is required for API Key authentication.");
        }

        if (options.MinLength < 0)
        {
            return ValidateOptionsResult.Fail("ApiKey:MinLength must be a non-negative integer.");
        }

        if (options.EnableRateLimiting)
        {
            if (options.RateLimitPerMinute <= 0)
            {
                return ValidateOptionsResult.Fail("ApiKey:RateLimitPerMinute must be greater than 0 when EnableRateLimiting is true.");
            }

            if (options.RateLimitWindowSeconds <= 0)
            {
                return ValidateOptionsResult.Fail("ApiKey:RateLimitWindowSeconds must be greater than 0 when EnableRateLimiting is true.");
            }
        }

        return ValidateOptionsResult.Success;
    }
}







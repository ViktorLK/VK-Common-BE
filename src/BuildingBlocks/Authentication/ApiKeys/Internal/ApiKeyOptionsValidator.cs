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
            return ValidateOptionsResult.Fail(ApiKeyConstants.HeaderNameRequired);
        }

        if (string.IsNullOrWhiteSpace(options.SchemeName))
        {
            return ValidateOptionsResult.Fail(ApiKeyConstants.SchemeNameRequired);
        }

        if (options.MinLength < 0)
        {
            return ValidateOptionsResult.Fail(ApiKeyConstants.MinLengthInvalid);
        }

        if (options.EnableRateLimiting)
        {
            if (options.RateLimitPerMinute <= 0)
            {
                return ValidateOptionsResult.Fail(ApiKeyConstants.RateLimitPerMinuteInvalid);
            }

            if (options.RateLimitWindowSeconds <= 0)
            {
                return ValidateOptionsResult.Fail(ApiKeyConstants.RateLimitWindowSecondsInvalid);
            }
        }

        return ValidateOptionsResult.Success;
    }
}

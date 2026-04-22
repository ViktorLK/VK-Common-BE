using Microsoft.Extensions.Options;

namespace VK.Blocks.Authentication.Jwt.Internal;

/// <summary>
/// Validates the <see cref="VKJwtOptions"/> options.
/// </summary>
internal sealed class JwtOptionsValidator : IValidateOptions<VKJwtOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, VKJwtOptions options)
    {
        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            return ValidateOptionsResult.Fail(JwtConstants.IssuerRequired);
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            return ValidateOptionsResult.Fail(JwtConstants.AudienceRequired);
        }

        if (options.ClockSkewSeconds < 0)
        {
            return ValidateOptionsResult.Fail(JwtConstants.ClockSkewInvalid);
        }

        if (options.AuthMode == VKJwtAuthMode.Symmetric)
        {
            if (string.IsNullOrWhiteSpace(options.SecretKey) || options.SecretKey.Length < JwtConstants.MinSecretKeyLength)
            {
                return ValidateOptionsResult.Fail(string.Format(JwtConstants.SecretKeyLengthInvalid, JwtConstants.MinSecretKeyLength));
            }

            if (options.ExpiryMinutes <= 0 || options.ExpiryMinutes > JwtConstants.MaxExpiryMinutes)
            {
                return ValidateOptionsResult.Fail(string.Format(JwtConstants.ExpiryRangeInvalid, JwtConstants.MaxExpiryMinutes));
            }

            if (options.RefreshTokenLifetimeDays <= 0 || options.RefreshTokenLifetimeDays > JwtConstants.MaxRefreshTokenLifetimeDays)
            {
                return ValidateOptionsResult.Fail(string.Format(JwtConstants.RefreshTokenRangeInvalid, JwtConstants.MaxRefreshTokenLifetimeDays));
            }
        }
        else if (options.AuthMode == VKJwtAuthMode.OidcDiscovery)
        {
            if (string.IsNullOrWhiteSpace(options.Authority))
            {
                return ValidateOptionsResult.Fail(JwtConstants.AuthorityRequired);
            }
        }
        else
        {
            return ValidateOptionsResult.Fail(JwtConstants.InvalidAuthMode);
        }

        return ValidateOptionsResult.Success;
    }
}

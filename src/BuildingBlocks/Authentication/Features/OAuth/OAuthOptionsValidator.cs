using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Common;

namespace VK.Blocks.Authentication.Features.OAuth;

/// <summary>
/// Validates the <see cref="OAuthOptions"/> during application startup.
/// </summary>
public sealed class OAuthOptionsValidator : IValidateOptions<OAuthOptions>
{
    #region Public Methods

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, OAuthOptions options)
    {
        foreach (var (providerName, providerOptions) in options.Providers)
        {
            if (!providerOptions.Enabled)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(providerOptions.Authority) ||
                !Uri.TryCreate(providerOptions.Authority, UriKind.Absolute, out _))
            {
                return ValidateOptionsResult.Fail(string.Format(AuthenticationConstants.OAuthAuthorityRequired, providerName));
            }

            if (string.IsNullOrWhiteSpace(providerOptions.ClientId))
            {
                return ValidateOptionsResult.Fail(string.Format(AuthenticationConstants.OAuthClientIdRequired, providerName));
            }

            if (string.IsNullOrWhiteSpace(providerOptions.CallbackPath) || !providerOptions.CallbackPath.StartsWith('/'))
            {
                return ValidateOptionsResult.Fail(string.Format(AuthenticationConstants.OAuthCallbackPathInvalid, providerName));
            }
        }

        return ValidateOptionsResult.Success;
    }

    #endregion
}

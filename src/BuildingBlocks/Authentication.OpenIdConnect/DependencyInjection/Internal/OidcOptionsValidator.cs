using Microsoft.Extensions.Options;

namespace VK.Blocks.Authentication.OpenIdConnect.DependencyInjection.Internal;

/// <summary>
/// Validator for VKOidcOptions to ensure correct configuration.
/// Complies with Rule 20.
/// </summary>
internal sealed class OidcOptionsValidator : IValidateOptions<VKOidcOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, VKOidcOptions options)
    {
        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        foreach (var pair in options.Providers)
        {
            var providerName = pair.Key;
            var provider = pair.Value;

            if (!provider.Enabled)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(provider.ClientId))
            {
                return ValidateOptionsResult.Fail($"OIDC provider '{providerName}' must have a ClientId.");
            }

            if (string.IsNullOrWhiteSpace(provider.Authority))
            {
                return ValidateOptionsResult.Fail($"OIDC provider '{providerName}' must have an Authority.");
            }

            if (string.IsNullOrWhiteSpace(provider.CallbackPath) || !provider.CallbackPath.StartsWith("/"))
            {
                return ValidateOptionsResult.Fail($"OIDC provider '{providerName}' must have a CallbackPath starting with '/'.");
            }
        }

        return ValidateOptionsResult.Success;
    }
}

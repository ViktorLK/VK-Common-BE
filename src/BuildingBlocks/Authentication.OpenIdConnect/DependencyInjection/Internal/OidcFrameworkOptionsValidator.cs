using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.OpenIdConnect.Oidc.Internal;

namespace VK.Blocks.Authentication.OpenIdConnect.Common.DependencyInjection.Internal;

/// <summary>
/// A validator that ensures each OpenIdConnectOptions configuration has a corresponding
/// VKOAuthProviderOptions entry in the VKAuthenticationOptions.
/// Complies with BB.05.
/// </summary>
internal sealed class OidcFrameworkOptionsValidator(
    IOptions<VKOidcDefaultsOptions> oidcOptions,
    ILogger<OidcFrameworkOptionsValidator> logger) : IValidateOptions<OpenIdConnectOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, OpenIdConnectOptions options)
    {
        // Null or default scheme name handling
        if (string.IsNullOrEmpty(name))
        {
            return ValidateOptionsResult.Success;
        }

        var failures = new List<string>();

        // We use the scheme name to find the provider configuration
        // In our setup, schemeName == providerName (or custom override)
        if (!oidcOptions.Value.Providers.TryGetValue(name, out var provider))
        {
            logger.LogOidcMappingError(name, OidcConstants.StartupTraceId);
            failures.Add(string.Format(OidcConstants.MissingConfigErrorMessage, name));
        }
        else
        {
            // Log successful registration ONCE during first resolution
            logger.LogOidcProviderRegistered(name, provider.Authority, OidcConstants.StartupTraceId);
        }

        if (failures.Count > 0)
        {
            return ValidateOptionsResult.Fail(failures);
        }

        return ValidateOptionsResult.Success;
    }
}

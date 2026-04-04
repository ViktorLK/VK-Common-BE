using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.DependencyInjection;

namespace VK.Blocks.Authentication.OpenIdConnect.Features.Oidc;

/// <summary>
/// A validator that ensures each OpenIdConnectOptions configuration has a corresponding 
/// OAuthProviderOptions entry in the VKAuthenticationOptions.
/// </summary>
internal sealed class OidcProviderValidator(
    IOptions<VKAuthenticationOptions> vkOptions,
    ILogger<OidcProviderValidator> logger) : IValidateOptions<OpenIdConnectOptions>
{
    #region Public Methods

    public ValidateOptionsResult Validate(string? name, OpenIdConnectOptions options)
    {
        // Null or default scheme name handling
        if (string.IsNullOrEmpty(name))
        {
            return ValidateOptionsResult.Success;
        }

        // We use the scheme name to find the provider configuration
        // In our setup, schemeName == providerName (or custom override)
        if (!vkOptions.Value.OAuth.Providers.TryGetValue(name, out var provider))
        {
            logger.LogOidcMappingError(name, OidcConstants.StartupTraceId);
            return ValidateOptionsResult.Fail(string.Format(OidcConstants.MissingConfigErrorMessage, name));
        }

        // Log successful registration ONCE during first resolution
        logger.LogOidcProviderRegistered(name, provider.Authority, OidcConstants.StartupTraceId);
        
        return ValidateOptionsResult.Success;
    }

    #endregion
}

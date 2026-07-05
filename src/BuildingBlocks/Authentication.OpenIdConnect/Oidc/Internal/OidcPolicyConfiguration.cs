using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.OpenIdConnect.Oidc.Internal;

/// <summary>
/// Automatically configures individual authorization policies for discovered OIDC providers.
/// Enables fine-grained authorization like [Authorize(Policy = "VK.Group.Google")].
/// </summary>
internal sealed class OidcPolicyConfiguration(
    IOptions<VKOidcOptions> oidcOptions,
    IOptions<VKOidcDefaultsOptions> defaultsOptions) : IConfigureOptions<AuthorizationOptions>
{
    private readonly IOptions<VKOidcOptions> _oidcOptions = VKGuard.NotNull(oidcOptions);
    private readonly IOptions<VKOidcDefaultsOptions> _defaultsOptions = VKGuard.NotNull(defaultsOptions);

    /// <inheritdoc />
    public void Configure(AuthorizationOptions options)
    {
        VKGuard.NotNull(options);
        var vkOidcOptions = _oidcOptions.Value;
        if (!vkOidcOptions.Enabled)
        {
            return;
        }

        var defaults = _defaultsOptions.Value;

        // We use the metadata generated specifically for the OIDC assembly.
        foreach (var pair in defaults.Providers)
        {
            var providerName = pair.Key;
            var providerOptions = pair.Value;

            if (providerOptions.Enabled)
            {
                var scheme = providerOptions.SchemeName ?? providerName;

                // Register Individual Provider Policy (e.g., "VK.Group.Google")
                options.AddPolicy($"{VKAuthenticationConstants.GroupPolicyPrefix}{providerName}", policy =>
                {
                    policy.AuthenticationSchemes.Add(scheme);
                    policy.RequireAuthenticatedUser();
                });
            }
        }
    }
}

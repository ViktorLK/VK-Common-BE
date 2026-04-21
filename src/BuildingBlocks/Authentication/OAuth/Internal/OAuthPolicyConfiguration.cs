using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Generated;

namespace VK.Blocks.Authentication.OAuth.Internal;

/// <summary>
/// Automatically configures individual authorization policies for discovered OAuth providers.
/// Enables fine-grained authorization like [Authorize(Policy = "VK.Group.GitHub")].
/// </summary>
internal sealed class OAuthPolicyConfiguration(IOptions<VKOAuthOptions> oauthOptions) : IConfigureOptions<AuthorizationOptions>
{
    /// <inheritdoc />
    public void Configure(AuthorizationOptions options)
    {
        VKOAuthOptions vkOAuthOptions = oauthOptions.Value;
        if (!vkOAuthOptions.Enabled)
        {
            return;
        }

        // We use the metadata generated at compile-time to discover available providers.
        // This keeps the registration logic decoupled from the central AuthenticationBlockExtensions.
        foreach (string providerName in VKOAuthGeneratedMetadata.AllProviders)
        {
            if (vkOAuthOptions.Providers.TryGetValue(providerName, out VKOAuthProviderOptions? providerOptions) && providerOptions.Enabled)
            {
                string scheme = providerOptions.SchemeName ?? providerName;

                // Register Individual Provider Policy (e.g., "VK.Group.GitHub")
                options.AddPolicy($"{VKAuthenticationConstants.GroupPolicyPrefix}{providerName}", policy =>
                {
                    policy.AuthenticationSchemes.Add(scheme);
                    policy.RequireAuthenticatedUser();
                });
            }
        }
    }
}

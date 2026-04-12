using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Common;
using VK.Blocks.Authentication.Generated;

namespace VK.Blocks.Authentication.Features.OAuth.Internal;

/// <summary>
/// Automatically configures individual authorization policies for discovered OAuth providers.
/// Enables fine-grained authorization like [Authorize(Policy = "VK.Group.GitHub")].
/// </summary>
internal sealed class OAuthPolicyConfiguration(IOptions<VKOAuthOptions> oauthOptions) : IConfigureOptions<AuthorizationOptions>
{
    public void Configure(AuthorizationOptions options)
    {
        var vkOAuthOptions = oauthOptions.Value;
        if (!vkOAuthOptions.Enabled)
        {
            return;
        }

        // We use the metadata generated at compile-time to discover available providers.
        // This keeps the registration logic decoupled from the central AuthenticationBlockExtensions.
        foreach (var providerName in VKOAuthGeneratedMetadata.AllProviders)
        {
            if (vkOAuthOptions.Providers.TryGetValue(providerName, out var providerOptions) && providerOptions.Enabled)
            {
                var scheme = providerOptions.SchemeName ?? providerName;
                
                // Register Individual Provider Policy (e.g., "VK.Group.GitHub")
                options.AddPolicy($"{AuthenticationConstants.GroupPolicyPrefix}{providerName}", policy =>
                {
                    policy.AuthenticationSchemes.Add(scheme);
                    policy.RequireAuthenticatedUser();
                });
            }
        }
    }
}

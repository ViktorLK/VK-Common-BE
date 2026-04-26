using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.OAuth.Internal;

/// <summary>
/// Publishes OAuth authentication schemes for semantic group policies.
/// </summary>
internal sealed class OAuthSemanticSchemeProvider(IOptions<VKOAuthOptions> options) : IVKSemanticSchemeProvider
{
    public IEnumerable<string> GetUserSchemes()
    {
        VKOAuthOptions oauthOptions = options.Value;
        if (!oauthOptions.Enabled)
        {
            return [];
        }

        return oauthOptions.Providers
            .Where(p => p.Value.Enabled)
            .Select(p => p.Value.SchemeName ?? p.Key);
    }

    public IEnumerable<string> GetServiceSchemes() => [];

    public IEnumerable<string> GetInternalSchemes() => [];

    public IEnumerable<string> GetSchemesForPolicy(string policyName)
    {
        if (policyName != VKAuthPolicies.OAuth)
        {
            return [];
        }

        VKOAuthOptions oauthOptions = options.Value;
        if (!oauthOptions.Enabled)
        {
            return [];
        }

        return oauthOptions.Providers
            .Where(p => p.Value.Enabled)
            .Select(p => p.Value.SchemeName ?? p.Key);
    }
}

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.OAuth.Internal;

/// <summary>
/// Publishes OAuth authentication schemes for semantic group policies.
/// </summary>
internal sealed class OAuthSemanticSchemeProvider(IOptionsMonitor<VKOAuthOptions> options) : IVKSemanticSchemeProvider
{
    public IEnumerable<string> GetUserSchemes()
    {
        VKOAuthOptions oauthOptions = options.CurrentValue;
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
}

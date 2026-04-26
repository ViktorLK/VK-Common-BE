using System.Collections.Generic;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.Jwt.Internal;

/// <summary>
/// Publishes JWT authentication schemes for semantic group policies.
/// </summary>
internal sealed class JwtSemanticSchemeProvider(IOptions<VKJwtOptions> options) : IVKSemanticSchemeProvider
{
    public IEnumerable<string> GetUserSchemes()
    {
        var jwt = options.Value;
        if (jwt.IsFeatureActivated())
        {
            yield return jwt.SchemeName;
        }
    }

    public IEnumerable<string> GetServiceSchemes()
    {
        var jwt = options.Value;
        if (jwt.IsFeatureActivated())
        {
            yield return jwt.SchemeName;
        }
    }

    public IEnumerable<string> GetInternalSchemes() => [];

    public IEnumerable<string> GetSchemesForPolicy(string policyName)
    {
        var jwt = options.Value;
        if (policyName == VKAuthPolicies.Jwt && jwt.IsFeatureActivated())
        {
            yield return jwt.SchemeName;
        }
    }
}

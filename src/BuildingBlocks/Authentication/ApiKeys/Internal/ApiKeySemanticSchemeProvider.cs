using System.Collections.Generic;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.ApiKeys.Internal;

/// <summary>
/// Publishes API key authentication schemes for semantic group policies.
/// </summary>
internal sealed class ApiKeySemanticSchemeProvider(IOptions<VKApiKeyOptions> options) : IVKSemanticSchemeProvider
{
    public IEnumerable<string> GetUserSchemes() => [];

    public IEnumerable<string> GetServiceSchemes()
    {
        VKApiKeyOptions apiKeyOptions = options.Value;
        if (apiKeyOptions.Enabled)
        {
            yield return apiKeyOptions.SchemeName;
        }
    }

    public IEnumerable<string> GetInternalSchemes()
    {
        VKApiKeyOptions apiKeyOptions = options.Value;
        if (apiKeyOptions.Enabled)
        {
            yield return apiKeyOptions.SchemeName;
        }
    }

    public IEnumerable<string> GetSchemesForPolicy(string policyName)
    {
        VKApiKeyOptions apiKeyOptions = options.Value;
        if (policyName == VKAuthPolicies.ApiKey && apiKeyOptions.Enabled)
        {
            yield return apiKeyOptions.SchemeName;
        }
    }
}

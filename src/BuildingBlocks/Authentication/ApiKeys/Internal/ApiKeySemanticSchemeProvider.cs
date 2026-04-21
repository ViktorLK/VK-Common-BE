using System.Collections.Generic;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.ApiKeys.Internal;

/// <summary>
/// Publishes API key authentication schemes for semantic group policies.
/// </summary>
internal sealed class ApiKeySemanticSchemeProvider(IOptionsMonitor<VKApiKeyOptions> options) : IVKSemanticSchemeProvider
{
    public IEnumerable<string> GetUserSchemes() => [];

    public IEnumerable<string> GetServiceSchemes()
    {
        VKApiKeyOptions apiKeyOptions = options.CurrentValue;
        if (apiKeyOptions.Enabled)
        {
            yield return apiKeyOptions.SchemeName;
        }
    }

    public IEnumerable<string> GetInternalSchemes()
    {
        VKApiKeyOptions apiKeyOptions = options.CurrentValue;
        if (apiKeyOptions.Enabled)
        {
            yield return apiKeyOptions.SchemeName;
        }
    }
}









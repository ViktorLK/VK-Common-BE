using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.Authentication.OpenIdConnect.Common.DependencyInjection.Internal;

/// <summary>
/// Hook implementation for Oidc Defaults validation.
/// </summary>
internal sealed partial class AuthenticationOpenIdConnectDefaultsFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKOidcDefaultsOptions options)
    {
        _ = services;
        _ = options;
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKOidcDefaultsOptions options, List<string> failures)
    {
        foreach (var pair in options.Providers)
        {
            var providerName = pair.Key;
            var provider = pair.Value;

            if (!provider.Enabled)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(provider.ClientId))
            {
                failures.Add($"OIDC provider '{providerName}' must have a ClientId.");
            }

            if (string.IsNullOrWhiteSpace(provider.Authority))
            {
                failures.Add($"OIDC provider '{providerName}' must have an Authority.");
            }

            if (string.IsNullOrWhiteSpace(provider.CallbackPath) || !provider.CallbackPath.StartsWith("/"))
            {
                failures.Add($"OIDC provider '{providerName}' must have a CallbackPath starting with '/'.");
            }
        }
    }
}

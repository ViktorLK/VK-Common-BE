using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Generated;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.OAuth.Internal;

/// <summary>
/// Partial implementation for OAuth feature hooks.
/// </summary>
internal sealed partial class OAuthFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKOAuthOptions options)
    {
        services.AddVKOAuthGeneratedMappers();
        services.AddSingleton<IConfigureOptions<AuthorizationOptions>, OAuthPolicyConfiguration>();

        // Publish schemes for semantic policies (IoC decoupling)
        services.TryAddEnumerableSingleton<IVKSemanticSchemeProvider, OAuthSemanticSchemeProvider>();
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKOAuthOptions options, List<string> failures)
    {
        if (options.Providers is null || options.Providers.Count is 0)
        {
            failures.Add(VKOAuthErrors.MissingProviders);
            return;
        }

        foreach ((string providerName, VKOAuthProviderOptions provider) in options.Providers.Where(p => p.Value.Enabled))
        {
            if (string.IsNullOrWhiteSpace(provider.ClientId))
            {
                failures.Add(string.Format(VKOAuthErrors.MissingClientIdTemplate, providerName));
            }

            if (string.IsNullOrWhiteSpace(provider.ClientSecret))
            {
                failures.Add(string.Format(VKOAuthErrors.MissingClientSecretTemplate, providerName));
            }

            if (string.IsNullOrWhiteSpace(provider.Authority))
            {
                failures.Add(string.Format(VKOAuthErrors.MissingAuthorityTemplate, providerName));
            }

            if (string.IsNullOrWhiteSpace(provider.CallbackPath))
            {
                failures.Add(string.Format(VKOAuthErrors.MissingCallbackPathTemplate, providerName));
            }
        }
    }
}

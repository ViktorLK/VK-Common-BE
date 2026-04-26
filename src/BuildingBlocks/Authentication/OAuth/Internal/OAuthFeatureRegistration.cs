using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Generated;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.OAuth.Internal;

/// <summary>
/// Internal registration logic for OAuth feature.
/// </summary>
internal static class OAuthFeatureRegistration
{
    internal static IVKAuthenticationBuilder Register(
        IVKAuthenticationBuilder builder,
        Func<VKOAuthOptions, VKOAuthOptions>? transform = null)
    {
        // 1. Check-Self (Rule 13 & 18.2)
        if (builder.Services.IsVKBlockRegistered<OAuthFeature>())
        {
            return builder;
        }

        // 2. Options Registration
        // ADR-016: Functional transformation from configuration section
        VKOAuthOptions oauthOptions = builder.Services.AddVKBlockOptions<VKOAuthOptions>(builder.Configuration, transform);

        // 3. Mark-Self (Rule 13)
        builder.Services.AddVKBlockMarker<OAuthFeature>();

        // Safety check: skip if parent block is disabled
        if (builder.AuthBuilder is null)
        {
            return builder;
        }

        IServiceCollection services = builder.Services;

        // Custom validators MUST use TryAddEnumerable to prevent being blocked by the built-in validators registered in AddVKBlockOptions.
        services.TryAddEnumerableSingleton<IValidateOptions<VKOAuthOptions>, OAuthOptionsValidator>();

        // 2. Dynamic OAuth Mappers
        if (oauthOptions.Enabled)
        {
            services.AddVKOAuthGeneratedMappers();
            services.AddSingleton<IConfigureOptions<AuthorizationOptions>, OAuthPolicyConfiguration>();

            // Publish schemes for semantic policies (IoC decoupling)
            services.TryAddEnumerableSingleton<IVKSemanticSchemeProvider, OAuthSemanticSchemeProvider>();
        }

        return builder;
    }
}

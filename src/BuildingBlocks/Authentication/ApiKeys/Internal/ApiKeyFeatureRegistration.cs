using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Common.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.ApiKeys.Internal;

/// <summary>
/// Internal registration logic for API Key feature.
/// </summary>
internal static class ApiKeyFeatureRegistration
{
    internal static IVKAuthenticationBuilder Register(IVKAuthenticationBuilder builder)
    {
        // 1. Options Registration
        builder.AddFeatureOptions<AuthenticationBlock, VKApiKeyOptions>();

        // Safety check: skip if parent block is disabled
        if (builder.AuthBuilder is null)
        {
            return builder;
        }

        VKApiKeyOptions apiKeyOptions = builder.Configuration.GetSection(VKApiKeyOptions.SectionName).Get<VKApiKeyOptions>() ?? new VKApiKeyOptions();
        IServiceCollection services = builder.Services;
        AuthenticationBuilder authBuilder = builder.AuthBuilder;

        // Custom validators MUST use TryAddEnumerable to prevent being blocked by the built-in validators registered in AddVKBlockOptions.
        services.TryAddEnumerableSingleton<IValidateOptions<VKApiKeyOptions>, ApiKeyOptionsValidator>();

        // 4. Feature Registration
        if (apiKeyOptions.Enabled)
        {
            // Scheme Registration
            authBuilder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(apiKeyOptions.SchemeName, apiKeyHandlerOptions =>
            {
                apiKeyHandlerOptions.HeaderName = apiKeyOptions.HeaderName;
            });

            // Core services and validation logic
            services.TryAddScoped<ApiKeyValidator>();

            // Infrastructure & Providers (e.g. Revocation and Rate Limiting)
            services.AddInMemoryCleanupProvider<IVKApiKeyRevocationProvider, InMemoryApiKeyRevocationProvider>(ServiceLifetime.Singleton);
            services.AddInMemoryCleanupProvider<IVKApiKeyRateLimiter, InMemoryApiKeyRateLimiter>(ServiceLifetime.Singleton);

            // Publish schemes for semantic policies (IoC decoupling)
            services.TryAddEnumerableSingleton<IVKSemanticSchemeProvider, ApiKeySemanticSchemeProvider>();
        }

        return builder;
    }
}

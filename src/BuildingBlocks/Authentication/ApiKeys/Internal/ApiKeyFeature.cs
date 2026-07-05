using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Authentication.ApiKeys.Internal;
using VK.Blocks.Authentication.Common.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.ApiKey.Internal;

/// <summary>
/// Partial implementation for API Key feature hooks.
/// </summary>
internal sealed partial class ApiKeyFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKApiKeyOptions options)
    {
        // Scheme Registration
        var authBuilder = services.AddAuthentication();
        authBuilder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(options.SchemeName, apiKeyHandlerOptions =>
        {
            apiKeyHandlerOptions.HeaderName = options.HeaderName;
        });

        // Core services and validation logic
        services.TryAddScoped<ApiKeyValidator>();

        // Infrastructure & Providers (e.g. Revocation and Rate Limiting)
        services.AddInMemoryCleanupProvider<IVKApiKeyRevocationProvider, InMemoryApiKeyRevocationProvider>(ServiceLifetime.Singleton);
        services.AddInMemoryCleanupProvider<IVKApiKeyRateLimiter, InMemoryApiKeyRateLimiter>(ServiceLifetime.Singleton);

        // Publish schemes for semantic policies (IoC decoupling)
        services.TryAddEnumerableSingleton<IVKSemanticSchemeProvider, ApiKeySemanticSchemeProvider>();
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKApiKeyOptions options, List<string> failures)
    {
        if (string.IsNullOrWhiteSpace(options.HeaderName))
        {
            failures.Add(ApiKeyConstants.HeaderNameRequired);
        }

        if (string.IsNullOrWhiteSpace(options.SchemeName))
        {
            failures.Add(ApiKeyConstants.SchemeNameRequired);
        }

        if (options.MinLength < 0)
        {
            failures.Add(ApiKeyConstants.MinLengthInvalid);
        }

        if (options.EnableRateLimiting)
        {
            if (options.RateLimitPerMinute <= 0)
            {
                failures.Add(ApiKeyConstants.RateLimitPerMinuteInvalid);
            }

            if (options.RateLimitWindowSeconds <= 0)
            {
                failures.Add(ApiKeyConstants.RateLimitWindowSecondsInvalid);
            }
        }
    }
}

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Common.Extensions;
using VK.Blocks.Authentication.Features.ApiKeys.Internal;
using VK.Blocks.Authentication.Features.ApiKeys.Persistence;
using VK.Blocks.Core.DependencyInjection;

namespace VK.Blocks.Authentication.Features.ApiKeys;

/// <summary>
/// Contains extension methods for registering API key authentication feature.
/// </summary>
public static class ApiKeyRegistration
{
    /// <summary>
    /// Adds API key authentication services to the container and registers the authentication scheme.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="apiKeySection">The configuration section for API key options.</param>
    /// <param name="authBuilder">The authentication builder to register the scheme.</param>
    /// <returns>The registered <see cref="ApiKeyOptions"/> instance.</returns>
    public static ApiKeyOptions AddApiKeysFeature(this IServiceCollection services, IConfigurationSection apiKeySection, AuthenticationBuilder authBuilder)
    {
        // 1. Options Registration
        var apiKeyOptions = services.AddVKBlockOptions<ApiKeyOptions>(apiKeySection);

        // Custom validators MUST use TryAddEnumerable to prevent being blocked by the built-in validators registered in AddVKBlockOptions.
        services.TryAddEnumerableSingleton<IValidateOptions<ApiKeyOptions>, ApiKeyOptionsValidator>();

        // 4. Feature Registration
        // We only register the scheme and associated services if the feature is enabled
        // to avoid unnecessary overhead in the pipeline and DI container.
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
            services.AddInMemoryCleanupProvider<IApiKeyRevocationProvider, InMemoryApiKeyRevocationProvider>(ServiceLifetime.Singleton);
            services.AddInMemoryCleanupProvider<IApiKeyRateLimiter, InMemoryApiKeyRateLimiter>(ServiceLifetime.Singleton);
        }

        return apiKeyOptions;
    }
}

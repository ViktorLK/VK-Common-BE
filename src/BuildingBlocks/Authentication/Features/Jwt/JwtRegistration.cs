using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Common.Extensions;
using VK.Blocks.Authentication.Features.Jwt.Internal;
using VK.Blocks.Authentication.Features.Jwt.Metadata;
using VK.Blocks.Authentication.Features.Jwt.Persistence;
using VK.Blocks.Core.DependencyInjection;

namespace VK.Blocks.Authentication.Features.Jwt;

/// <summary>
/// Contains extension methods for registering JWT authentication feature.
/// </summary>
public static class JwtRegistration
{
    /// <summary>
    /// Adds JWT authentication services to the container and registers the Bearer authentication scheme.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="jwtSection">The configuration section for JWT options.</param>
    /// <param name="authBuilder">The authentication builder to register the scheme.</param>
    /// <returns>The registered <see cref="JwtOptions"/> instance.</returns>
    public static JwtOptions AddJwtFeature(this IServiceCollection services, IConfigurationSection jwtSection, AuthenticationBuilder authBuilder)
    {
        // 1. Options Registration
        var jwtOptions = services.AddVKBlockOptions<JwtOptions>(jwtSection);

        // Custom validators MUST use TryAddEnumerable to prevent being blocked by the built-in validators registered in AddVKBlockOptions.
        services.TryAddEnumerableSingleton<IValidateOptions<JwtOptions>, JwtOptionsValidator>();

        // 2. Conditional Feature Registration
        // We evaluate activation logic based on required parameters for the chosen AuthMode.
        if (jwtOptions.IsFeatureActivated())
        {
            // Base Infrastructure & Validation Services
            services.AddInMemoryCleanupProvider<IJwtTokenRevocationProvider, InMemoryJwtTokenRevocationProvider>(ServiceLifetime.Singleton);
            services.TryAddScoped<IJwtAuthenticationService, JwtAuthenticationService>();
            services.TryAddScoped<IJwtTokenRevocationService, JwtTokenRevocationService>();
            services.AddInMemoryCleanupProvider<IJwtRefreshTokenValidator, InMemoryJwtRefreshTokenValidator>(ServiceLifetime.Singleton);

            // JwtBearer Scheme Configuration
            authBuilder.AddJwtBearer(jwtOptions.SchemeName, jwtBearerOptions =>
            {
                if (jwtOptions.AuthMode == JwtAuthMode.OidcDiscovery)
                {
                    jwtBearerOptions.Authority = jwtOptions.Authority;
                    if (!string.IsNullOrEmpty(jwtOptions.MetadataAddress))
                    {
                        jwtBearerOptions.MetadataAddress = jwtOptions.MetadataAddress;
                    }
                }

                jwtBearerOptions.TokenValidationParameters = JwtValidationFactory.Create(jwtOptions);
                jwtBearerOptions.Events = JwtEventsFactory.CreateEvents();
            });
        }

        return jwtOptions;
    }

    /// <summary>
    /// Determines whether the JWT feature should be activated based on configuration.
    /// </summary>
    internal static bool IsFeatureActivated(this JwtOptions jwtOptions)
    {
        return jwtOptions.Enabled && jwtOptions.AuthMode switch
        {
            JwtAuthMode.Symmetric => !string.IsNullOrEmpty(jwtOptions.SecretKey),
            JwtAuthMode.OidcDiscovery => !string.IsNullOrEmpty(jwtOptions.Authority),
            _ => false
        };
    }
}



using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Common.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.Jwt.Internal;

/// <summary>
/// Internal registration logic for JWT feature.
/// </summary>
internal static class JwtFeatureRegistration
{
    internal static IVKAuthenticationBuilder Register(IVKAuthenticationBuilder builder)
    {
        // 1. Check-Self (Rule 13 & 18.2)
        if (builder.Services.IsVKBlockRegistered<JwtFeature>())
        {
            return builder;
        }

        // 2. Options Registration
        VKJwtOptions jwtOptions = builder.Services.AddVKBlockOptions<VKJwtOptions>(builder.Configuration);

        // 3. Mark-Self (Rule 13)
        builder.Services.AddVKBlockMarker<JwtFeature>();

        // Safety check: skip if parent block is disabled
        if (builder.AuthBuilder is null)
        {
            return builder;
        }

        IServiceCollection services = builder.Services;
        AuthenticationBuilder authBuilder = builder.AuthBuilder;

        // Custom validators MUST use TryAddEnumerable to prevent being blocked by the built-in validators registered in AddVKBlockOptions.
        services.TryAddEnumerableSingleton<IValidateOptions<VKJwtOptions>, JwtOptionsValidator>();

        // 2. Conditional Feature Registration
        if (jwtOptions.IsFeatureActivated())
        {
            // Base Infrastructure & Validation Services
            services.TryAddScoped<IVKJwtAuthService, JwtAuthenticationService>();
            services.TryAddScoped<IVKJwtRevocationService, JwtTokenRevocationService>();
            services.AddInMemoryCleanupProvider<IVKJwtRevocationProvider, InMemoryJwtTokenRevocationProvider>(ServiceLifetime.Singleton);
            services.AddInMemoryCleanupProvider<IVKJwtRefreshValidator, InMemoryJwtRefreshTokenValidator>(ServiceLifetime.Singleton);

            // Publish schemes for semantic policies (IoC decoupling)
            services.TryAddEnumerableSingleton<IVKSemanticSchemeProvider, JwtSemanticSchemeProvider>();

            // JwtBearer Scheme Configuration
            authBuilder.AddJwtBearer(jwtOptions.SchemeName, jwtBearerOptions =>
            {
                if (jwtOptions.AuthMode == VKJwtAuthMode.OidcDiscovery)
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

        return builder;
    }
}

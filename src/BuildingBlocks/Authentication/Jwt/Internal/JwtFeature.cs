using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Authentication.Common.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.Jwt.Internal;

/// <summary>
/// Partial implementation for JWT feature hooks.
/// </summary>
internal sealed partial class JwtFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKJwtOptions options)
    {
        // Base Infrastructure & Validation Services
        services.TryAddScoped<IVKJwtAuthService, JwtAuthenticationService>();
        services.TryAddScoped<IVKJwtRevocationService, JwtTokenRevocationService>();
        services.AddInMemoryCleanupProvider<IVKJwtRevocationProvider, InMemoryJwtTokenRevocationProvider>(ServiceLifetime.Singleton);
        services.AddInMemoryCleanupProvider<IVKJwtRefreshValidator, InMemoryJwtRefreshTokenValidator>(ServiceLifetime.Singleton);

        // Publish schemes for semantic policies (IoC decoupling)
        services.TryAddEnumerableSingleton<IVKSemanticSchemeProvider, JwtSemanticSchemeProvider>();

        // JwtBearer Scheme Configuration
        var authBuilder = services.AddAuthentication();
        authBuilder.AddJwtBearer(options.SchemeName, jwtBearerOptions =>
        {
            if (options.AuthMode == VKJwtAuthMode.OidcDiscovery)
            {
                jwtBearerOptions.Authority = options.Authority;
                if (!string.IsNullOrEmpty(options.MetadataAddress))
                {
                    jwtBearerOptions.MetadataAddress = options.MetadataAddress;
                }
            }

            jwtBearerOptions.TokenValidationParameters = JwtValidationFactory.Create(options);
            jwtBearerOptions.Events = JwtEventsFactory.CreateEvents();
        });
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKJwtOptions options, List<string> failures)
    {
        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            failures.Add(JwtConstants.IssuerRequired);
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            failures.Add(JwtConstants.AudienceRequired);
        }

        if (options.ClockSkewSeconds < 0)
        {
            failures.Add(JwtConstants.ClockSkewInvalid);
        }

        if (options.AuthMode == VKJwtAuthMode.Symmetric)
        {
            if (string.IsNullOrWhiteSpace(options.SecretKey) || options.SecretKey.Length < JwtConstants.MinSecretKeyLength)
            {
                failures.Add(string.Format(JwtConstants.SecretKeyLengthInvalid, JwtConstants.MinSecretKeyLength));
            }

            if (options.ExpiryMinutes <= 0 || options.ExpiryMinutes > JwtConstants.MaxExpiryMinutes)
            {
                failures.Add(string.Format(JwtConstants.ExpiryRangeInvalid, JwtConstants.MaxExpiryMinutes));
            }

            if (options.RefreshTokenLifetimeDays <= 0 || options.RefreshTokenLifetimeDays > JwtConstants.MaxRefreshTokenLifetimeDays)
            {
                failures.Add(string.Format(JwtConstants.RefreshTokenRangeInvalid, JwtConstants.MaxRefreshTokenLifetimeDays));
            }
        }
        else if (options.AuthMode == VKJwtAuthMode.OidcDiscovery)
        {
            if (string.IsNullOrWhiteSpace(options.Authority))
            {
                failures.Add(JwtConstants.AuthorityRequired);
            }
        }
        else
        {
            failures.Add(JwtConstants.InvalidAuthMode);
        }
    }
}

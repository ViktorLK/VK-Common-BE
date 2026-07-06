using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication;

/// <summary>
/// Extension methods for configuring sub-features on the authentication builder.
/// Following BB.06.
/// </summary>
public static partial class VKAuthenticationBuilderExtensions
{
    /// <summary>
    /// Registers a custom JWT refresh token validator.
    /// </summary>
    public static IVKAuthenticationBuilder WithJwtRefreshTokenValidator<T>(this IVKAuthenticationBuilder builder)
        where T : class, IVKJwtRefreshValidator
    {
        VKGuard.NotNull(builder);
        builder.WithSingleton<VKAuthenticationBlock, IVKJwtRefreshValidator, T>();
        return builder;
    }

    /// <summary>
    /// Registers a custom JWT token revocation provider.
    /// </summary>
    public static IVKAuthenticationBuilder WithJwtTokenRevocationProvider<T>(this IVKAuthenticationBuilder builder)
        where T : class, IVKJwtRevocationProvider
    {
        VKGuard.NotNull(builder);
        builder.WithSingleton<VKAuthenticationBlock, IVKJwtRevocationProvider, T>();
        return builder;
    }

    /// <summary>
    /// Registers a custom API key revocation provider.
    /// </summary>
    public static IVKAuthenticationBuilder WithApiKeyRevocationProvider<T>(this IVKAuthenticationBuilder builder)
        where T : class, IVKApiKeyRevocationProvider
    {
        VKGuard.NotNull(builder);
        builder.WithSingleton<VKAuthenticationBlock, IVKApiKeyRevocationProvider, T>();
        return builder;
    }

    /// <summary>
    /// Registers a custom API key rate limiter.
    /// </summary>
    public static IVKAuthenticationBuilder WithApiKeyRateLimiter<T>(this IVKAuthenticationBuilder builder)
        where T : class, IVKApiKeyRateLimiter
    {
        VKGuard.NotNull(builder);
        builder.WithSingleton<VKAuthenticationBlock, IVKApiKeyRateLimiter, T>();
        return builder;
    }

    /// <summary>
    /// Registers a custom claims provider for enriching the authenticated principal (idempotent addition).
    /// </summary>
    public static IVKAuthenticationBuilder TryAddClaimsProvider<T>(this IVKAuthenticationBuilder builder)
        where T : class, IVKClaimsProvider
    {
        VKGuard.NotNull(builder);
        builder.TryAddEnumerableScoped<VKAuthenticationBlock, IVKClaimsProvider, T>();
        return builder;
    }

    /// <summary>
    /// Adds a custom OAuth claims mapper for a specific provider (idempotent addition).
    /// </summary>
    public static IVKAuthenticationBuilder TryAddOAuthMapper<TMapper>(this IVKAuthenticationBuilder builder, string providerName)
        where TMapper : class, IVKOAuthClaimsMapper
    {
        VKGuard.NotNull(builder);
        builder.Services.TryAddKeyedScoped<IVKOAuthClaimsMapper, TMapper>(providerName);
        return builder;
    }
}

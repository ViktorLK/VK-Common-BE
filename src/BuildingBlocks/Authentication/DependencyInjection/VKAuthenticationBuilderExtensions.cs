using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication;

/// <summary>
/// Provides extension methods for <see cref="IVKBlockBuilder{AuthenticationBlock}"/> to configure the authentication block.
/// </summary>
public static class VKAuthenticationBuilderExtensions
{
    /// <summary>
    /// Registers a custom JWT refresh token validator.
    /// </summary>
    public static IVKBlockBuilder<AuthenticationBlock> WithJwtRefreshTokenValidator<T>(this IVKBlockBuilder<AuthenticationBlock> builder)
        where T : class, IVKJwtRefreshValidator
        => builder.WithSingleton<AuthenticationBlock, IVKJwtRefreshValidator, T>();

    /// <summary>
    /// Registers a custom JWT token revocation provider.
    /// </summary>
    public static IVKBlockBuilder<AuthenticationBlock> WithJwtTokenRevocationProvider<T>(this IVKBlockBuilder<AuthenticationBlock> builder)
        where T : class, IVKJwtRevocationProvider
        => builder.WithSingleton<AuthenticationBlock, IVKJwtRevocationProvider, T>();

    /// <summary>
    /// Registers a custom API key revocation provider.
    /// </summary>
    public static IVKBlockBuilder<AuthenticationBlock> WithApiKeyRevocationProvider<T>(this IVKBlockBuilder<AuthenticationBlock> builder)
        where T : class, IVKApiKeyRevocationProvider
        => builder.WithSingleton<AuthenticationBlock, IVKApiKeyRevocationProvider, T>();

    /// <summary>
    /// Registers a custom API key rate limiter.
    /// </summary>
    public static IVKBlockBuilder<AuthenticationBlock> WithApiKeyRateLimiter<T>(this IVKBlockBuilder<AuthenticationBlock> builder)
        where T : class, IVKApiKeyRateLimiter
        => builder.WithSingleton<AuthenticationBlock, IVKApiKeyRateLimiter, T>();

    /// <summary>
    /// Registers a custom claims provider for enriching the authenticated principal (idempotent addition).
    /// </summary>
    public static IVKBlockBuilder<AuthenticationBlock> TryAddClaimsProvider<T>(this IVKBlockBuilder<AuthenticationBlock> builder)
        where T : class, IVKClaimsProvider
        => builder.TryAddEnumerableScoped<AuthenticationBlock, IVKClaimsProvider, T>();

    /// <summary>
    /// Adds a custom OAuth claims mapper for a specific provider (idempotent addition).
    /// </summary>
    public static IVKBlockBuilder<AuthenticationBlock> TryAddOAuthMapper<TMapper>(this IVKBlockBuilder<AuthenticationBlock> builder, string providerName)
        where TMapper : class, IVKOAuthClaimsMapper
    {
        builder.Services.TryAddKeyedScoped<IVKOAuthClaimsMapper, TMapper>(providerName);
        return builder;
    }
}









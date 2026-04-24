using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication;

/// <summary>
/// Provides extension methods for <see cref="IVKBlockBuilder{VKAuthenticationBlock}"/> to configure the authentication block.
/// </summary>
public static class VKAuthenticationBuilderExtensions
{
    /// <summary>
    /// Registers a custom JWT refresh token validator.
    /// </summary>
    public static IVKBlockBuilder<VKAuthenticationBlock> WithJwtRefreshTokenValidator<T>(this IVKBlockBuilder<VKAuthenticationBlock> builder)
        where T : class, IVKJwtRefreshValidator
        => VKGuard.NotNull(builder).WithSingleton<VKAuthenticationBlock, IVKJwtRefreshValidator, T>();

    /// <summary>
    /// Registers a custom JWT token revocation provider.
    /// </summary>
    public static IVKBlockBuilder<VKAuthenticationBlock> WithJwtTokenRevocationProvider<T>(this IVKBlockBuilder<VKAuthenticationBlock> builder)
        where T : class, IVKJwtRevocationProvider
        => VKGuard.NotNull(builder).WithSingleton<VKAuthenticationBlock, IVKJwtRevocationProvider, T>();

    /// <summary>
    /// Registers a custom API key revocation provider.
    /// </summary>
    public static IVKBlockBuilder<VKAuthenticationBlock> WithApiKeyRevocationProvider<T>(this IVKBlockBuilder<VKAuthenticationBlock> builder)
        where T : class, IVKApiKeyRevocationProvider
        => VKGuard.NotNull(builder).WithSingleton<VKAuthenticationBlock, IVKApiKeyRevocationProvider, T>();

    /// <summary>
    /// Registers a custom API key rate limiter.
    /// </summary>
    public static IVKBlockBuilder<VKAuthenticationBlock> WithApiKeyRateLimiter<T>(this IVKBlockBuilder<VKAuthenticationBlock> builder)
        where T : class, IVKApiKeyRateLimiter
        => VKGuard.NotNull(builder).WithSingleton<VKAuthenticationBlock, IVKApiKeyRateLimiter, T>();

    /// <summary>
    /// Registers a custom claims provider for enriching the authenticated principal (idempotent addition).
    /// </summary>
    public static IVKBlockBuilder<VKAuthenticationBlock> TryAddClaimsProvider<T>(this IVKBlockBuilder<VKAuthenticationBlock> builder)
        where T : class, IVKClaimsProvider
        => VKGuard.NotNull(builder).TryAddEnumerableScoped<VKAuthenticationBlock, IVKClaimsProvider, T>();

    /// <summary>
    /// Adds a custom OAuth claims mapper for a specific provider (idempotent addition).
    /// </summary>
    public static IVKBlockBuilder<VKAuthenticationBlock> TryAddOAuthMapper<TMapper>(this IVKBlockBuilder<VKAuthenticationBlock> builder, string providerName)
        where TMapper : class, IVKOAuthClaimsMapper
    {
        builder.Services.TryAddKeyedScoped<IVKOAuthClaimsMapper, TMapper>(providerName);
        return builder;
    }
}

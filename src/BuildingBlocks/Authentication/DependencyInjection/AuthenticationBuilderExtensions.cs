using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Authentication.Abstractions;
using VK.Blocks.Authentication.Features.ApiKeys.Persistence;
using VK.Blocks.Authentication.Features.Jwt.Persistence;
using VK.Blocks.Authentication.Features.OAuth;
using VK.Blocks.Core.DependencyInjection;

namespace VK.Blocks.Authentication.DependencyInjection;

/// <summary>
/// Provides extension methods for <see cref="IVKBlockBuilder{AuthenticationBlock}"/> to configure the authentication block.
/// </summary>
public static class AuthenticationBuilderExtensions
{
    #region Public Methods

    /// <summary>
    /// Registers a custom JWT refresh token validator.
    /// </summary>
    public static IVKBlockBuilder<AuthenticationBlock> WithJwtRefreshTokenValidator<T>(this IVKBlockBuilder<AuthenticationBlock> builder)
        where T : class, IJwtRefreshTokenValidator
        => builder.WithSingleton<AuthenticationBlock, IJwtRefreshTokenValidator, T>();

    /// <summary>
    /// Registers a custom JWT token revocation provider.
    /// </summary>
    public static IVKBlockBuilder<AuthenticationBlock> WithJwtTokenRevocationProvider<T>(this IVKBlockBuilder<AuthenticationBlock> builder)
        where T : class, IJwtTokenRevocationProvider
        => builder.WithSingleton<AuthenticationBlock, IJwtTokenRevocationProvider, T>();

    /// <summary>
    /// Registers a custom API key revocation provider.
    /// </summary>
    public static IVKBlockBuilder<AuthenticationBlock> WithApiKeyRevocationProvider<T>(this IVKBlockBuilder<AuthenticationBlock> builder)
        where T : class, IApiKeyRevocationProvider
        => builder.WithSingleton<AuthenticationBlock, IApiKeyRevocationProvider, T>();

    /// <summary>
    /// Registers a custom API key rate limiter.
    /// </summary>
    public static IVKBlockBuilder<AuthenticationBlock> WithApiKeyRateLimiter<T>(this IVKBlockBuilder<AuthenticationBlock> builder)
        where T : class, IApiKeyRateLimiter
        => builder.WithSingleton<AuthenticationBlock, IApiKeyRateLimiter, T>();

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
        where TMapper : class, IOAuthClaimsMapper
    {
        builder.Services.TryAddKeyedScoped<IOAuthClaimsMapper, TMapper>(providerName);
        return builder;
    }

    #endregion
}

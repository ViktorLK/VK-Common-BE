using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Authentication.Abstractions;
using VK.Blocks.Authentication.Features.ApiKeys;
using VK.Blocks.Authentication.Features.Jwt.RefreshTokens;
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
    public static IVKBlockBuilder<AuthenticationBlock> AddJwtRefreshTokenValidator<T>(this IVKBlockBuilder<AuthenticationBlock> builder)
        where T : class, IJwtRefreshTokenValidator
    {
        builder.Services.Replace(ServiceDescriptor.Singleton<IJwtRefreshTokenValidator, T>());
        return builder;
    }

    /// <summary>
    /// Registers a custom JWT token revocation provider.
    /// </summary>
    public static IVKBlockBuilder<AuthenticationBlock> AddJwtTokenRevocationProvider<T>(this IVKBlockBuilder<AuthenticationBlock> builder)
        where T : class, IJwtTokenRevocationProvider
    {
        builder.Services.Replace(ServiceDescriptor.Singleton<IJwtTokenRevocationProvider, T>());
        return builder;
    }

    /// <summary>
    /// Registers a custom API key revocation provider.
    /// </summary>
    public static IVKBlockBuilder<AuthenticationBlock> AddApiKeyRevocationProvider<T>(this IVKBlockBuilder<AuthenticationBlock> builder)
        where T : class, IApiKeyRevocationProvider
    {
        builder.Services.Replace(ServiceDescriptor.Singleton<IApiKeyRevocationProvider, T>());
        return builder;
    }

    /// <summary>
    /// Registers a custom API key rate limiter.
    /// </summary>
    public static IVKBlockBuilder<AuthenticationBlock> AddApiKeyRateLimiter<T>(this IVKBlockBuilder<AuthenticationBlock> builder)
        where T : class, IApiKeyRateLimiter
    {
        builder.Services.Replace(ServiceDescriptor.Singleton<IApiKeyRateLimiter, T>());
        return builder;
    }

    /// <summary>
    /// Registers a custom claims provider for enriching the authenticated principal.
    /// </summary>
    public static IVKBlockBuilder<AuthenticationBlock> AddClaimsProvider<T>(this IVKBlockBuilder<AuthenticationBlock> builder)
        where T : class, IVKClaimsProvider
    {
        // Claims providers are usually additive, but the transformer currently takes the last one registered.
        // We use AddScoped to allow implementation-specific dependencies.
        builder.Services.AddScoped<IVKClaimsProvider, T>();
        return builder;
    }

    #endregion
}

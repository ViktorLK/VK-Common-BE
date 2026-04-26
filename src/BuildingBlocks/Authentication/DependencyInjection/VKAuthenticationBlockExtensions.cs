using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Authentication.ApiKeys.Internal;
using VK.Blocks.Authentication.DependencyInjection.Internal;
using VK.Blocks.Authentication.Jwt.Internal;
using VK.Blocks.Authentication.OAuth.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication;

/// <summary>
/// Extension methods for configuring authentication block services.
/// </summary>
public static class VKAuthenticationBlockExtensions
{
    /// <summary>
    /// Adds the VK authentication block configuration to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    public static IVKAuthenticationBuilder AddVKAuthenticationBlock(this IServiceCollection services, IConfiguration configuration)
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(configuration);
        return AuthenticationBlockRegistration.Register(services, configuration);
    }

    /// <summary>
    /// Adds the VK authentication block to the specified <see cref="IServiceCollection"/> using a functional transformation setup.
    /// Following ADR-016: Use 'with' expression to modify immutable options.
    /// </summary>
    public static IVKAuthenticationBuilder AddVKAuthenticationBlock(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<VKAuthenticationOptions, VKAuthenticationOptions> configure)
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(configuration);
        VKGuard.NotNull(configure);
        return AuthenticationBlockRegistration.Register(services, configuration, configure);
    }

    /// <summary>
    /// Adds JWT authentication services to the container and registers the Bearer authentication scheme.
    /// Following ADR-016: Supports immutable options (init) via 'with' expressions.
    /// </summary>
    public static IVKAuthenticationBuilder AddVKJwt(
        this IVKAuthenticationBuilder builder,
        Func<VKJwtOptions, VKJwtOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        return JwtFeatureRegistration.Register(builder, transform);
    }

    /// <summary>
    /// Adds OAuth authentication services and dynamic mappers to the container.
    /// Following ADR-016: Supports immutable options (init) via 'with' expressions.
    /// </summary>
    public static IVKAuthenticationBuilder AddVKOAuth(
        this IVKAuthenticationBuilder builder,
        Func<VKOAuthOptions, VKOAuthOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        return OAuthFeatureRegistration.Register(builder, transform);
    }

    /// <summary>
    /// Adds API key authentication services to the container and registers the authentication scheme.
    /// Following ADR-016: Supports immutable options (init) via 'with' expressions.
    /// </summary>
    public static IVKAuthenticationBuilder AddVKApiKeys(
        this IVKAuthenticationBuilder builder,
        Func<VKApiKeyOptions, VKApiKeyOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        return ApiKeyFeatureRegistration.Register(builder, transform);
    }

    /// <summary>
    /// Automatically enables all standard authentication features (Jwt, ApiKeys, OAuth).
    /// </summary>
    public static IVKAuthenticationBuilder AddVKDefaultFeatures(this IVKAuthenticationBuilder builder)
    {
        VKGuard.NotNull(builder);
        return builder
            .AddVKJwt()
            .AddVKApiKeys()
            .AddVKOAuth();
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Authentication.ApiKeys.Internal;
using VK.Blocks.Authentication.DependencyInjection.Internal;
using VK.Blocks.Authentication.Jwt.Internal;
using VK.Blocks.Authentication.OAuth.Internal;

namespace VK.Blocks.Authentication;

/// <summary>
/// Extension methods for configuring authentication block services.
/// </summary>
public static class VKAuthenticationBlockExtensions
{
    /// <summary>
    /// Adds the VK authentication block configuration to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    public static IVKAuthenticationBuilder AddAuthenticationBlock(this IServiceCollection services, IConfiguration configuration)
    {
        return AuthenticationBlockRegistration.Register(services, configuration);
    }

    /// <summary>
    /// Adds JWT authentication services to the container and registers the Bearer authentication scheme.
    /// </summary>
    public static IVKAuthenticationBuilder AddVKJwt(this IVKAuthenticationBuilder builder)
        => JwtFeatureRegistration.Register(builder);

    /// <summary>
    /// Adds OAuth authentication services and dynamic mappers to the container.
    /// </summary>
    public static IVKAuthenticationBuilder AddVKOAuth(this IVKAuthenticationBuilder builder)
        => OAuthFeatureRegistration.Register(builder);

    /// <summary>
    /// Adds API key authentication services to the container and registers the authentication scheme.
    /// </summary>
    public static IVKAuthenticationBuilder AddVKApiKeys(this IVKAuthenticationBuilder builder)
        => ApiKeyFeatureRegistration.Register(builder);
}

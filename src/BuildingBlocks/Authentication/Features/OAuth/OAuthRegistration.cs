using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Features.OAuth.Internal;
using VK.Blocks.Authentication.Generated;
using VK.Blocks.Core.DependencyInjection;

namespace VK.Blocks.Authentication.Features.OAuth;

/// <summary>
/// Contains extension methods for registering OAuth authentication feature.
/// </summary>
public static class OAuthRegistration
{
    /// <summary>
    /// Adds OAuth authentication services and dynamic mappers to the container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="oauthSection">The configuration section for OAuth options.</param>
    /// <returns>The registered <see cref="VKOAuthOptions"/> instance.</returns>
    public static VKOAuthOptions AddOAuthFeature(this IServiceCollection services, IConfigurationSection oauthSection)
    {
        // 1. Options Registration
        var vkOAuthOptions = services.AddVKBlockOptions<VKOAuthOptions>(oauthSection);

        // Custom validators MUST use TryAddEnumerable to prevent being blocked by the built-in validators registered in AddVKBlockOptions.
        services.TryAddEnumerableSingleton<IValidateOptions<VKOAuthOptions>, VKOAuthOptionsValidator>();

        // 2. Dynamic OAuth Mappers
        // We register OAuth claims mappers discovered by the source generator at compile time.
        // Registration is conditional to keep the DI container lean when OAuth is disabled.
        if (vkOAuthOptions.Enabled)
        {
            services.AddVKOAuthGeneratedMappers();
            services.AddSingleton<IConfigureOptions<AuthorizationOptions>, OAuthPolicyConfiguration>();
        }

        return vkOAuthOptions;
    }
}

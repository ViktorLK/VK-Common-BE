using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.DependencyInjection;
using VK.Blocks.Authentication.Features.OAuth;
using VK.Blocks.Authentication.Generated;
using VK.Blocks.Authentication.OpenIdConnect.Features.Oidc.Internal;
using VK.Blocks.Core.DependencyInjection;

namespace VK.Blocks.Authentication.OpenIdConnect.Features.Oidc;

/// <summary>
/// Contains extension methods for registering OIDC authentication feature.
/// </summary>
public static class OidcRegistration
{
    /// <summary>
    /// Discovers and registers OAuth providers with Fail-Fast validation.
    /// </summary>
    internal static IServiceCollection AddOidcFeature(this IServiceCollection services, IConfiguration configuration)
    {
        var authSection = configuration.GetSection(VKAuthenticationOptions.SectionName);
        var authOptions = services.AddVKBlockOptions<VKAuthenticationOptions>(authSection);

        if (!authOptions.Enabled)
        {
            return services;
        }

        // HttpClient for OIDC
        services.AddHttpClient(OidcConstants.OidcBackchannelName);

        var vkOAuthOptions = services.AddVKBlockOptions<VKOAuthOptions>(authSection.GetSection(VKAuthenticationOptions.OAuthSection));

        if (!vkOAuthOptions.Enabled || !vkOAuthOptions.Providers.Any(p => p.Value.Enabled))
        {
            return services;
        }

        // Register mappers from the OIDC assembly
        services.AddVKOidcGeneratedMappers();

        // Register OIDC-specific policy configuration
        services.AddSingleton<IConfigureOptions<AuthorizationOptions>, OidcPolicyConfiguration>();

        var authBuilder = services.AddAuthentication();

        // Custom validators MUST use TryAddEnumerable to prevent being blocked by the built-in validators registered in AddVKBlockOptions.
        services.TryAddEnumerableSingleton<IValidateOptions<OpenIdConnectOptions>, OidcProviderValidator>();

        foreach (var (providerName, providerOptions) in vkOAuthOptions.Providers.Where(p => p.Value.Enabled))
        {
            var schemeName = providerOptions.SchemeName ?? providerName;

            authBuilder.AddOpenIdConnect(schemeName, _ => { });

            services.AddOptions<OpenIdConnectOptions>(schemeName)
                .Configure<IOptionsMonitor<VKAuthenticationOptions>, IHttpClientFactory>((oidcOptions, authOptionsMonitor, httpClientFactory) =>
                {
                    if (!authOptionsMonitor.CurrentValue.OAuth.Providers.TryGetValue(providerName, out var pOptions))
                    {
                        return;
                    }

                    oidcOptions.Authority = pOptions.Authority;
                    oidcOptions.ClientId = pOptions.ClientId;
                    oidcOptions.ClientSecret = pOptions.ClientSecret;
                    oidcOptions.CallbackPath = pOptions.CallbackPath;
                    oidcOptions.ResponseType = pOptions.ResponseType ?? OidcConstants.DefaultResponseType;
                    oidcOptions.SaveTokens = true;
                    oidcOptions.GetClaimsFromUserInfoEndpoint = pOptions.GetClaimsFromUserInfoEndpoint;

                    oidcOptions.Scope.Clear();
                    foreach (var scope in pOptions.Scopes)
                    {
                        oidcOptions.Scope.Add(scope);
                    }

                    oidcOptions.Backchannel = httpClientFactory.CreateClient(OidcConstants.OidcBackchannelName);

                    oidcOptions.Events = new OpenIdConnectEvents
                    {
                        OnTokenValidated = OidcHandlerFactory.CreateOnTokenValidated(providerName)
                    };
                });
        }

        return services;
    }
}

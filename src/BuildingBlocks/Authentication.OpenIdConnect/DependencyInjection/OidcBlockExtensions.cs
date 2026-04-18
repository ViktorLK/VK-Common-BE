using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Common;
using VK.Blocks.Authentication.Contracts;
using VK.Blocks.Authentication.DependencyInjection;
using VK.Blocks.Authentication.Features.OAuth;
using VK.Blocks.Authentication.Generated;
using VK.Blocks.Authentication.OpenIdConnect.Contracts;
using VK.Blocks.Authentication.OpenIdConnect.Features.Oidc.Internal;
using VK.Blocks.Core.DependencyInjection;

namespace VK.Blocks.Authentication.OpenIdConnect.DependencyInjection;

/// <summary>
/// Extension methods for configuring OpenIdConnect authentication block.
/// Complies with Rule 13 (DI Registration Pattern).
/// </summary>
public static class OidcBlockExtensions
{
    /// <summary>
    /// Adds OIDC block to the authentication pipeline.
    /// </summary>
    /// <param name="builder">The authentication block builder.</param>
    /// <param name="configuration">The root configuration.</param>
    /// <returns>The same builder instance.</returns>
    public static IVKBlockBuilder<AuthenticationBlock> AddVKOidcBlock(this IVKBlockBuilder<AuthenticationBlock> builder, IConfiguration configuration)
    {
        var services = builder.Services;

        // 1. Prerequisites & Idempotency Check (Rule 13)
        if (services.IsVKBlockRegistered<OidcBlock>())
        {
            return builder;
        }

        // 2. Validate prerequisites
        services.EnsureVKBlockRegistered<AuthenticationBlock, OidcBlock>();

        // 2. Options Resolution
        // Rule 15: Bind options from the root configuration.
        // We use GetSection().Get<T>() manually here if we want to avoid side-effects on the collection before the enabled check,
        // but AddVKBlockOptions is the architecturally standard way. We'll stick to it and ensure marker is the success commit.
        var authOptions = services.AddVKBlockOptions<VKAuthenticationOptions>(configuration);
        var authSection = configuration.GetSection(VKAuthenticationOptions.SectionName);

        // Success Commit (Marker)
        // Mark as registered after options are bound but before feature-gate check.
        services.AddVKBlockMarker<OidcBlock>();

        if (!authOptions.Enabled)
        {
            return builder;
        }

        var vkOAuthOptions = authOptions.OAuth;
        if (!vkOAuthOptions.Enabled || !vkOAuthOptions.Providers.Any(p => p.Value.Enabled))
        {
            return builder;
        }

        // 3. Feature Registration (HttpClient & Mappers)
        services.AddHttpClient(OidcConstants.OidcBackchannelName);
        services.AddVKOidcGeneratedMappers();

        // Access OAuth options safely from the already-bound root options.
        services.Configure<VKOAuthOptions>(authSection.GetSection(VKAuthenticationOptions.OAuthSection));

        // 4. Authorization Policy Configuration
        services.AddSingleton<IConfigureOptions<AuthorizationOptions>, OidcPolicyConfiguration>();

        // Ensure the Standard policy is registered if enabled, independent of Source Generator metadata.
        // This provides a reliable baseline for tests and the framework itself.
        if (vkOAuthOptions.Providers.TryGetValue(OidcConstants.StandardProvider, out var standardOptions) && standardOptions.Enabled)
        {
            services.PostConfigure<AuthorizationOptions>(options =>
            {
                var scheme = standardOptions.SchemeName ?? OidcConstants.StandardProvider;
                options.AddPolicy($"{AuthenticationConstants.GroupPolicyPrefix}{OidcConstants.StandardProvider}", policy =>
                {
                    policy.AuthenticationSchemes.Add(scheme);
                    policy.RequireAuthenticatedUser();
                });
            });
        }

        // 5. Authentication Framework Integration
        var authBuilder = services.AddAuthentication();
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

                    // Map provider configuration to OIDC options
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

                    // Attach standardized identity extraction events
                    oidcOptions.Events = new OpenIdConnectEvents
                    {
                        OnTokenValidated = OidcHandlerFactory.CreateOnTokenValidated(providerName)
                    };
                });
        }

        return builder;
    }
}




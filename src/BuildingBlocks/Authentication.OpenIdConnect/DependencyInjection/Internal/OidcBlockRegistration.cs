using System;
using System.Linq;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Generated;
using VK.Blocks.Authentication.OpenIdConnect.Diagnostics.Internal;
using VK.Blocks.Authentication.OpenIdConnect.Oidc.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.OpenIdConnect.Common.DependencyInjection.Internal;

// [SG Registration]
internal static partial class OidcBlockRegistration
{
    // [SG Hook]
    static partial void RegisterBlockCustom(IVKOidcBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        // Register defaults feature
        AuthenticationOpenIdConnectDefaultsFeature.Register(builder);

        // Register custom validators
        services.TryAddEnumerableSingleton<IValidateOptions<OpenIdConnectOptions>, OidcFrameworkOptionsValidator>();

        // Diagnostics
        services.TryAddSingleton<IVKSecurityMetadataProvider, OidcMetadataProvider>();

        // Retrieve options configured so far
        var oidcOptions = services.GetVKServiceInstance<VKOidcOptions>();
        if (oidcOptions == null || !oidcOptions.Enabled)
        {
            return;
        }

        var defaults = services.GetVKServiceInstance<VKOidcDefaultsOptions>();
        if (defaults == null)
        {
            return;
        }

        if (defaults.Providers.Count == 0 || defaults.Providers.All(p => !p.Value.Enabled))
        {
            return;
        }

        // Mappers (Source Generated)
        services.AddVKOidcGeneratedMappers();

        // Standard IDP Registration
        foreach (var pair in defaults.Providers)
        {
            var providerName = pair.Key;
            var providerOptions = pair.Value;

            if (!providerOptions.Enabled)
            {
                continue;
            }

            var schemeName = providerOptions.SchemeName ?? providerName;

            services.AddAuthentication()
                .AddOpenIdConnect(schemeName, options =>
                {
                    options.Authority = providerOptions.Authority;
                    options.ClientId = providerOptions.ClientId;
                    options.ClientSecret = providerOptions.ClientSecret;
                    options.CallbackPath = providerOptions.CallbackPath;
                    options.SaveTokens = defaults.SaveTokens;
                    options.RequireHttpsMetadata = defaults.RequireHttpsMetadata;

                    if (defaults.BackchannelTimeoutSeconds > 0)
                    {
                        options.BackchannelTimeout = TimeSpan.FromSeconds(defaults.BackchannelTimeoutSeconds);
                    }

                    if (providerOptions.ResponseType is not null)
                    {
                        options.ResponseType = providerOptions.ResponseType;
                    }

                    foreach (var scope in providerOptions.Scopes)
                    {
                        options.Scope.Add(scope);
                    }

                    options.GetClaimsFromUserInfoEndpoint = providerOptions.GetClaimsFromUserInfoEndpoint;

                    // Standard Event Handlers
                    options.Events.OnTokenValidated = OidcHandlerFactory.CreateOnTokenValidated(providerName);
                });
        }

        // Global Policy Configuration
        services.TryAddSingleton<IConfigureOptions<AuthorizationOptions>, OidcPolicyConfiguration>();
    }
}

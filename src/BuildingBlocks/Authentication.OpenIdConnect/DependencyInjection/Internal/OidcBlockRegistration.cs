using System;
using System.Linq;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Generated;
using VK.Blocks.Authentication.OpenIdConnect.Diagnostics.Internal;
using VK.Blocks.Authentication.OpenIdConnect.Oidc.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.OpenIdConnect.DependencyInjection.Internal;

/// <summary>
/// Principal registration logic for the OpenIdConnect building block.
/// Complies with Rule 18.2 (Internal Core).
/// </summary>
internal static class OidcBlockRegistration
{
    public static IVKBlockBuilder<VKAuthenticationBlock> Register(
        IVKBlockBuilder<VKAuthenticationBlock> builder,
        IConfiguration configuration,
        Func<VKOidcOptions, VKOidcOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        VKGuard.NotNull(configuration);

        var services = builder.Services;

        // 1. Check-Self & Prerequisite (Rule 18.2.1)
        if (services.IsVKBlockRegistered<VKOidcBlock>())
        {
            return builder;
        }

        // 2. Options Registration (Rule 18.2.2)
        // ADR-016: Use functional transformation to support immutable options
        VKOidcOptions oidcOptions = services.AddVKBlockOptions<VKOidcOptions>(configuration, transform);

        // 3. Mark-Self (Rule 18.2.3)
        services.AddVKBlockMarker<VKOidcBlock>();

        // 4. Options Validation (Rule 18.2.4)
        services.TryAddEnumerableSingleton<IValidateOptions<VKOidcOptions>, OidcOptionsValidator>();
        services.TryAddEnumerableSingleton<IValidateOptions<OpenIdConnectOptions>, OidcFrameworkOptionsValidator>();

        // 5. Diagnostics (Rule 18.2.5)
        services.TryAddSingleton<IVKSecurityMetadataProvider, OidcMetadataProvider>();

        // 6. Feature Toggle (Rule 18.2.6)
        if (!oidcOptions.Enabled)
        {
            return builder;
        }

        if (oidcOptions.Providers.Count == 0 || oidcOptions.Providers.All(p => !p.Value.Enabled))
        {
            return builder;
        }

        // 7. Core Services (Rule 18.2.7)
        RegisterCoreServices(services, oidcOptions);

        return builder;
    }

    private static void RegisterCoreServices(
        IServiceCollection services,
        VKOidcOptions oidcOptions)
    {
        // Mappers (Source Generated)
        services.AddVKOidcGeneratedMappers();

        // Standard IDP Registration
        foreach (var pair in oidcOptions.Providers)
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
                    options.SaveTokens = oidcOptions.SaveTokens;
                    options.RequireHttpsMetadata = oidcOptions.RequireHttpsMetadata;

                    if (oidcOptions.BackchannelTimeoutSeconds > 0)
                    {
                        options.BackchannelTimeout = TimeSpan.FromSeconds(oidcOptions.BackchannelTimeoutSeconds);
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

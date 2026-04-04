using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using VK.Blocks.Authentication.DependencyInjection;
using VK.Blocks.Authentication.Features.OAuth;
using VK.Blocks.Authentication.OpenIdConnect.Features.Oidc;
using VK.Blocks.Core.DependencyInjection;

namespace VK.Blocks.Authentication.OpenIdConnect.DependencyInjection;

/// <summary>
/// Extension methods for configuring OpenId Connect authentication with minimalist design.
/// Resilience (e.g. Polly) should be configured at the application level.
/// </summary>
public static class OidcAuthenticationBuilderExtensions
{
    #region Fields

    /// <summary>
    /// Global backchannel name for OIDC communication.
    /// Consumers can use this name to apply custom HttpClient policies (Retries, Timeouts).
    /// </summary>
    public const string OidcBackchannelName = "OidcBackchannel";

    #endregion

    #region Public Methods

    /// <summary>
    /// Discovers and registers OAuth providers with Fail-Fast validation.
    /// Resilience policies should be added externally to the "OidcBackchannel" HttpClient.
    /// </summary>
    /// <param name="builder">The authentication block builder.</param>
    /// <param name="configuration">The configuration to discover providers from.</param>
    /// <returns>The builder for chaining.</returns>
    public static IVKBlockBuilder<AuthenticationBlock> AddDiscoveryOAuth(this IVKBlockBuilder<AuthenticationBlock> builder, IConfiguration configuration)
    {
        var services = builder.Services;

        // 0. Fail-Fast & Idempotency:
        // a) If this block is already registered, skip to avoid redundant logic.
        if (services.IsVKBlockRegistered<OidcBlock>())
        {
            return builder;
        }

        // b) Ensure the core Authentication block is registered as a prerequisite.
        if (!services.IsVKBlockRegistered<AuthenticationBlock>())
        {
            throw new InvalidOperationException(OidcConstants.DependencyMissingMessage);
        }

        var section = configuration.GetSection(VKAuthenticationOptions.SectionName);

        // 1. Fail-Fast Validation Setup (Standard Core Pattern)
        // Register root options for global switches (e.g. Enabled, DefaultScheme).
        var vkOptions = services.AddVKBlockOptions<VKAuthenticationOptions>(section);

        // 2. Global Enable Check
        if (!vkOptions.Enabled)
        {
            return builder;
        }

        // 3. Register standard HttpClient for OIDC (without direct Polly dependency)
        services.AddHttpClient(OidcBackchannelName);

        // 4. Discover and register providers using standard VKOAuthOptions binding.
        // Explicitly register VKOAuthOptions as a standalone service to allow direct injection
        // of IOptions<VKOAuthOptions> and trigger specific OIDC-level validations.
        var vkOAuthOptions = services.AddVKBlockOptions<VKOAuthOptions>(section.GetSection(VKAuthenticationOptions.OAuthSection));

        // Skip registration if OAuth is disabled or no providers are enabled.
        if (!vkOAuthOptions.Enabled || !vkOAuthOptions.Providers.Any(p => p.Value.Enabled))
        {
            return builder;
        }

        var authBuilder = services.AddAuthentication();

        // Register the dedicated validator for OIDC schemes to handle startup logging and mapping validation safely.
        services.TryAddSingleton<IValidateOptions<OpenIdConnectOptions>, OidcProviderValidator>();

        foreach (var (providerName, providerOptions) in vkOAuthOptions.Providers.Where(p => p.Value.Enabled))
        {
            var schemeName = providerOptions.SchemeName ?? providerName;

            // Register the scheme placeholder. Actual configuration is performed via AddOptions().Configure below
            // to allow dynamic binding with IOptionsSnapshot and IHttpClientFactory.
            authBuilder.AddOpenIdConnect(schemeName, _ => { });

            services.AddOptions<OpenIdConnectOptions>(schemeName)
                .Configure<IOptionsMonitor<VKAuthenticationOptions>, IHttpClientFactory>((options, vkOptionsMonitor, httpClientFactory) =>
                {
                    // Capture 'providerName' from the loop for deferred lookup.
                    // Note: In C# 5+, foreach variables are iteration-scoped, making this capture safe.
                    if (!vkOptionsMonitor.CurrentValue.OAuth.Providers.TryGetValue(providerName, out var p))
                    {
                        return;
                    }

                    options.Authority = p.Authority;
                    options.ClientId = p.ClientId;
                    options.ClientSecret = p.ClientSecret;
                    options.CallbackPath = p.CallbackPath;
                    options.ResponseType = p.ResponseType ?? OidcConstants.DefaultResponseType;
                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = p.GetClaimsFromUserInfoEndpoint;

                    options.Scope.Clear();
                    foreach (var scope in p.Scopes)
                    {
                        options.Scope.Add(scope);
                    }

                    // Use the application-customizable backchannel
                    options.Backchannel = httpClientFactory.CreateClient(OidcBackchannelName);

                    options.Events = new OpenIdConnectEvents
                    {
                        OnTokenValidated = OidcHandlerFactory.CreateOnTokenValidated(providerName)
                    };
                });
        }

        // 5. Mark-Self (Success Commit)
        services.AddVKBlockMarker<OidcBlock>();

        return builder;
    }

    #endregion
}

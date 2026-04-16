using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Abstractions;
using VK.Blocks.Authentication.Common;
using VK.Blocks.Authentication.Diagnostics;
using VK.Blocks.Authentication.Features.ApiKeys;
using VK.Blocks.Authentication.Features.Jwt;
using VK.Blocks.Authentication.Features.OAuth;
using VK.Blocks.Authentication.Generated;
using VK.Blocks.Core.DependencyInjection;
using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.Authentication.DependencyInjection;

/// <summary>
/// Extension methods for configuring authentication block services.
/// </summary>
public static class AuthenticationBlockExtensions
{
    /// <summary>
    /// Adds the VK authentication block configuration to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    public static IVKBlockBuilder<AuthenticationBlock> AddVKAuthenticationBlock(this IServiceCollection services, IConfiguration configuration)
    {
        // 0. Idempotency & Prerequisite Check
        if (services.IsVKBlockRegistered<AuthenticationBlock>())
        {
            return new VKBlockBuilder<AuthenticationBlock>(services);
        }

        if (!services.IsVKBlockRegistered<CoreBlock>())
        {
            throw new InvalidOperationException(
                string.Format(CoreConstants.MissingCoreRegistrationMessage, typeof(AuthenticationBlock).Assembly.GetName().Name));
        }

        var authOptions = services.AddVKBlockOptions<VKAuthenticationOptions>(configuration);
        var authSection = configuration.GetSection(VKAuthenticationOptions.SectionName);
        
        // Initialize builder earlier to use idempotent registration helpers
        var builder = new VKBlockBuilder<AuthenticationBlock>(services);

        builder.TryAddEnumerableSingleton<AuthenticationBlock, IValidateOptions<VKAuthenticationOptions>, VKAuthenticationOptionsValidator>();

        // Skip configuration of authentication schemes and handlers if the block is globally disabled.
        // NOTE: We register root block options before this check to ensure metadata is available.
        if (!authOptions.Enabled)
        {
            return builder;
        }

        // 2. Core Authentication Setup
        var authBuilder = services.AddCoreAuthenticationFramework(authOptions);

        // 3. Feature Configuration (Delegated to Vertical Slices)
        // Each feature handles its own options registration, internal services, and scheme registration.
        // Rule 13: We use the returned options instances for conditional logic to avoid redundant ServiceProvider builds.
        var jwtOptions = services.AddJwtFeature(authSection.GetSection(VKAuthenticationOptions.JwtSection), authBuilder);
        var apiKeyOptions = services.AddApiKeysFeature(authSection.GetSection(VKAuthenticationOptions.ApiKeySection), authBuilder);
        var vkOAuthOptions = services.AddOAuthFeature(authSection.GetSection(VKAuthenticationOptions.OAuthSection));

        // 4. Semantic Authorization Policies
        // We define high-level policies that combine multiple authentication strategies.
        services.AddSemanticAuthorizationPolicies(jwtOptions, apiKeyOptions, vkOAuthOptions);

        // 5. Core Authentication Setup Completion
        // We ensure our transformer is registered LAST to ensure it takes precedence over
        // any defaults (like NoopClaimsTransformation) registered by framework extensions.
        services.TryAddTransient<IClaimsTransformation, VKClaimsTransformer>();

        // 6. Discovery & Auto-Registration
        // We register any IVKClaimsProvider implementations discovered by the source generator.
        services.AddGeneratedClaimsProviders();

        // Register the security metadata provider for web discovery
        services.TryAddEnumerableSingleton<ISecurityMetadataProvider, AuthenticationMetadataProvider>();

        // 7. Mark-Self (Success Commit)
        services.AddVKBlockMarker<AuthenticationBlock>();

        return builder;
    }

    private static AuthenticationBuilder AddCoreAuthenticationFramework(
        this IServiceCollection services,
        VKAuthenticationOptions authOptions)
    {
        services.AddHttpContextAccessor();

        return services.AddAuthentication(authSetupOptions =>
        {
            authSetupOptions.DefaultAuthenticateScheme = authOptions.DefaultScheme;
            authSetupOptions.DefaultChallengeScheme = authOptions.DefaultScheme;
        });
    }

    private static void AddSemanticAuthorizationPolicies(
        this IServiceCollection services,
        JwtOptions jwtOptions,
        ApiKeyOptions apiKeyOptions,
        VKOAuthOptions vkOAuthOptions)
    {
        var isJwtActivated = jwtOptions.IsFeatureActivated();

        services.AddAuthorization(authPolicyOptions =>
        {
            // Individual Strategy Policies
            if (isJwtActivated)
            {
                authPolicyOptions.AddPolicy(AuthenticationConstants.JwtPolicy, policy =>
                {
                    policy.AuthenticationSchemes.Add(jwtOptions.SchemeName);
                    policy.RequireAuthenticatedUser();
                });
            }

            if (apiKeyOptions.Enabled)
            {
                authPolicyOptions.AddPolicy(AuthenticationConstants.ApiKeyPolicy, policy =>
                {
                    policy.AuthenticationSchemes.Add(apiKeyOptions.SchemeName);
                    policy.RequireAuthenticatedUser();
                });
            }

            // --- Authentication Groups (Rule 14: Cross-feature coordination) ---

            // Group: User (Supports human-facing schemes: JWT/OIDC)
            var hasUserSchemes = isJwtActivated || (vkOAuthOptions.Enabled && vkOAuthOptions.Providers.Any(p => p.Value.Enabled));
            if (hasUserSchemes)
            {
                authPolicyOptions.AddPolicy($"{AuthenticationConstants.GroupPolicyPrefix}{AuthGroups.User}", policy =>
                {
                    if (isJwtActivated)
                    {
                        policy.AuthenticationSchemes.Add(jwtOptions.SchemeName);
                    }

                    if (vkOAuthOptions.Enabled)
                    {
                        foreach (var (providerName, providerOptions) in vkOAuthOptions.Providers)
                        {
                            if (providerOptions.Enabled)
                            {
                                var scheme = providerOptions.SchemeName ?? providerName;
                                policy.AuthenticationSchemes.Add(scheme);
                            }
                        }
                    }

                    policy.RequireAuthenticatedUser();
                });
            }

            // Group: Service (Supports machine-to-machine schemes: ApiKey and fallback JWT)
            if (apiKeyOptions.Enabled || isJwtActivated)
            {
                authPolicyOptions.AddPolicy($"{AuthenticationConstants.GroupPolicyPrefix}{AuthGroups.Service}", policy =>
                {
                    if (apiKeyOptions.Enabled)
                    {
                        policy.AuthenticationSchemes.Add(apiKeyOptions.SchemeName);
                    }

                    if (isJwtActivated)
                    {
                        policy.AuthenticationSchemes.Add(jwtOptions.SchemeName);
                    }

                    policy.RequireAuthenticatedUser();
                });
            }

            // Group: Internal (High-trust, typically ApiKey only)
            if (apiKeyOptions.Enabled)
            {
                authPolicyOptions.AddPolicy($"{AuthenticationConstants.GroupPolicyPrefix}{AuthGroups.Internal}", policy =>
                {
                    policy.AuthenticationSchemes.Add(apiKeyOptions.SchemeName);
                    policy.RequireAuthenticatedUser();
                });
            }
        });
    }
}

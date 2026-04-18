using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Abstractions;
using VK.Blocks.Authentication.Common;
using VK.Blocks.Authentication.Contracts;
using VK.Blocks.Authentication.Diagnostics;
using VK.Blocks.Authentication.Features.ApiKeys;
using VK.Blocks.Authentication.Features.Jwt;
using VK.Blocks.Authentication.Features.OAuth;
using VK.Blocks.Authentication.Generated;
using VK.Blocks.Core.DependencyInjection;
using VK.Blocks.Core.Security;

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
        // Prerequisites & Idempotency Check
        if (services.IsVKBlockRegistered<AuthenticationBlock>())
        {
            return new VKBlockBuilder<AuthenticationBlock>(services);
        }

        // 2. Validate prerequisites
        services.EnsureVKCoreBlockRegistered<AuthenticationBlock>();

        // Options Registration
        var authOptions = services.AddVKBlockOptions<VKAuthenticationOptions>(configuration);
        var authSection = configuration.GetSection(VKAuthenticationOptions.SectionName);

        var builder = new VKBlockBuilder<AuthenticationBlock>(services);
        builder.TryAddEnumerableSingleton<AuthenticationBlock, IValidateOptions<VKAuthenticationOptions>, VKAuthenticationOptionsValidator>();

        // Success Commit (Marker)
        // Mark as initialized early to allow dependent blocks (like OIDC) to proceed
        // even if the core features are disabled in configuration.
        services.AddVKBlockMarker<AuthenticationBlock>();

        // Static Diagnostic Metadata
        services.TryAddEnumerableSingleton<ISecurityMetadataProvider, AuthenticationMetadataProvider>();

        // Feature Activation Check
        if (!authOptions.Enabled)
        {
            return builder;
        }

        // Core Infrastructure
        services.TryAddTransient<IClaimsTransformation, VKClaimsTransformer>();

        var authBuilder = services.AddCoreAuthenticationFramework(authOptions);

        // Feature Registration (Vertical Slices)
        var jwtOptions = services.AddJwtFeature(authSection.GetSection(VKAuthenticationOptions.JwtSection), authBuilder);
        var apiKeyOptions = services.AddApiKeysFeature(authSection.GetSection(VKAuthenticationOptions.ApiKeySection), authBuilder);
        var vkOAuthOptions = services.AddOAuthFeature(authSection.GetSection(VKAuthenticationOptions.OAuthSection));

        // Group Authorization & Auto-Discovery
        services.AddSemanticAuthorizationPolicies(jwtOptions, apiKeyOptions, vkOAuthOptions);
        services.AddGeneratedClaimsProviders();

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







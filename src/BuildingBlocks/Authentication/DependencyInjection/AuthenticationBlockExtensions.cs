using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Abstractions;
using VK.Blocks.Authentication.Common;
using VK.Blocks.Authentication.Features.ApiKeys;
using VK.Blocks.Authentication.Features.Jwt;
using VK.Blocks.Authentication.Features.Jwt.RefreshTokens;
using VK.Blocks.Authentication.Features.OAuth;
using VK.Blocks.Authentication.Generated;
using VK.Blocks.Core.DependencyInjection;

namespace VK.Blocks.Authentication.DependencyInjection;

/// <summary>
/// Extension methods for configuring authentication block services.
/// </summary>
public static class AuthenticationBlockExtensions
{
    #region Public Methods

    /// <summary>
    /// Adds the VK authentication block configuration to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    public static IVKBlockBuilder<AuthenticationBlock> AddVKAuthenticationBlock(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton(TimeProvider.System);
        var section = configuration.GetSection(VKAuthenticationOptions.SectionName);

        // 1. Standard Block Registration (Eager-bind, Singleton, DataAnnotations, ValidateOnStart)
        var authOptions = services.AddVKBlockOptions<VKAuthenticationOptions>(section);
        services.AddSingleton<IValidateOptions<VKAuthenticationOptions>, VKAuthenticationOptionsValidator>();

        // 2. JWT Validation Registration
        var jwtOptions = services.AddVKBlockOptions<JwtOptions>(section.GetSection(VKAuthenticationOptions.JwtSection));
        services.AddSingleton<IValidateOptions<JwtOptions>, JwtOptionsValidator>();

        // 3. API Key Registration
        var apiKeyOptions = services.AddVKBlockOptions<ApiKeyOptions>(section.GetSection(VKAuthenticationOptions.ApiKeySection));
        services.AddSingleton<IValidateOptions<ApiKeyOptions>, ApiKeyOptionsValidator>();

        // 4. OAuth Registration
        var oauthOptions = services.AddVKBlockOptions<OAuthOptions>(section.GetSection(VKAuthenticationOptions.OAuthSection));
        services.AddSingleton<IValidateOptions<OAuthOptions>, OAuthOptionsValidator>();

        // Skip configuration of authentication schemes and handlers if the block is globally disabled.
        // NOTE: We register all strategy-specific options (JWT, ApiKey, OAuth) before this check
        // to ensure that configuration metadata is always available in the DI container
        // for inspection or monitoring, preventing dependency resolution errors in other components.
        if (!authOptions.Enabled)
        {
            return new VKBlockBuilder<AuthenticationBlock>(services);
        }

        // 1. Core Authentication Setup (JWT + API Key schemes)
        var authBuilder = services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = authOptions.DefaultScheme;
            options.DefaultChallengeScheme = authOptions.DefaultScheme;
        });

        // 2. JWT Configuration (Using settings already registered above)

        var shouldRegisterJwt = jwtOptions.Enabled && jwtOptions.AuthMode switch
        {
            JwtAuthMode.Symmetric => !string.IsNullOrEmpty(jwtOptions.SecretKey),
            JwtAuthMode.OidcDiscovery => !string.IsNullOrEmpty(jwtOptions.Authority),
            _ => false
        };

        // 2. JWT Configuration

        if (shouldRegisterJwt)
        {
            // Base Infrastructure & Validation Services
            services.AddInMemoryCleanupProvider<IJwtTokenRevocationProvider, InMemoryJwtTokenRevocationProvider>(ServiceLifetime.Singleton);
            services.TryAddScoped<IJwtAuthenticationService, JwtAuthenticationService>();
            services.TryAddScoped<IJwtTokenRevocationService, JwtTokenRevocationService>();
            services.AddInMemoryCleanupProvider<IJwtRefreshTokenValidator, InMemoryJwtRefreshTokenValidator>(ServiceLifetime.Singleton);

            authBuilder.AddJwtBearer(jwtOptions.SchemeName, options =>
            {
                if (jwtOptions.AuthMode == JwtAuthMode.OidcDiscovery)
                {
                    options.Authority = jwtOptions.Authority;
                    if (!string.IsNullOrEmpty(jwtOptions.MetadataAddress))
                    {
                        options.MetadataAddress = jwtOptions.MetadataAddress;
                    }
                }

                options.TokenValidationParameters = JwtValidationFactory.Create(jwtOptions);
                options.Events = JwtEventsFactory.CreateEvents();
            });
        }

        // 3. API Key Configuration (Using settings already registered above)
        if (apiKeyOptions.Enabled)
        {
            authBuilder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(apiKeyOptions.SchemeName, options =>
            {
                options.HeaderName = apiKeyOptions.HeaderName;
            });

            services.TryAddScoped<ApiKeyValidator>();

            services.AddInMemoryCleanupProvider<IApiKeyRevocationProvider, InMemoryApiKeyRevocationProvider>(ServiceLifetime.Singleton);
            services.AddInMemoryCleanupProvider<IApiKeyRateLimiter, InMemoryApiKeyRateLimiter>(ServiceLifetime.Singleton);
        }

        // 4. OAuth Mappers
        // Register OAuth claims mappers dynamically based on [OAuthProvider] attribute
        if (oauthOptions.Enabled)
        {
            services.AddGeneratedOAuthMappers();
        }

        // 5. Global Pipeline Refinement (Claims Transformation)
        services.AddHttpContextAccessor();
        services.TryAddTransient<IClaimsTransformation, VKClaimsTransformer>();

        // 6. Semantic Authorization Policies
        services.AddAuthorization(options =>
        {
            // Individual Strategies
            if (shouldRegisterJwt)
            {
                options.AddPolicy(AuthenticationConstants.JwtPolicy, policy =>
                {
                    policy.AuthenticationSchemes.Add(jwtOptions.SchemeName);
                    policy.RequireAuthenticatedUser();
                });
            }

            if (apiKeyOptions.Enabled)
            {
                options.AddPolicy(AuthenticationConstants.ApiKeyPolicy, policy =>
                {
                    policy.AuthenticationSchemes.Add(apiKeyOptions.SchemeName);
                    policy.RequireAuthenticatedUser();
                });
            }

            // --- Authentication Groups (Combining schemes) ---

            // Group: User (Typical for human users, supports JWT/OIDC)
            var hasUserSchemes = shouldRegisterJwt || (oauthOptions.Enabled && oauthOptions.Providers.Any(p => p.Value.Enabled));
            if (hasUserSchemes)
            {
                options.AddPolicy($"{AuthenticationConstants.GroupPolicyPrefix}{AuthGroups.User}", policy =>
                {
                    if (shouldRegisterJwt)
                    {
                        policy.AuthenticationSchemes.Add(jwtOptions.SchemeName);
                    }

                    if (oauthOptions.Enabled)
                    {
                        foreach (var (providerName, providerOptions) in oauthOptions.Providers)
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

            // Group: Service (Typical for machine-to-machine, supports ApiKey and fallback JWT)
            if (apiKeyOptions.Enabled || shouldRegisterJwt)
            {
                options.AddPolicy($"{AuthenticationConstants.GroupPolicyPrefix}{AuthGroups.Service}", policy =>
                {
                    if (apiKeyOptions.Enabled)
                    {
                        policy.AuthenticationSchemes.Add(apiKeyOptions.SchemeName);
                    }

                    if (shouldRegisterJwt)
                    {
                        policy.AuthenticationSchemes.Add(jwtOptions.SchemeName);
                    }

                    policy.RequireAuthenticatedUser();
                });
            }

            // Group: Internal (High-trust, typically ApiKey only)
            if (apiKeyOptions.Enabled)
            {
                options.AddPolicy($"{AuthenticationConstants.GroupPolicyPrefix}{AuthGroups.Internal}", policy =>
                {
                    policy.AuthenticationSchemes.Add(apiKeyOptions.SchemeName);
                    policy.RequireAuthenticatedUser();
                });
            }
        });

        return new VKBlockBuilder<AuthenticationBlock>(services);
    }

    /// <summary>
    /// Adds a custom OAuth claims mapper to the authentication block.
    /// </summary>
    /// <typeparam name="TMapper">The type of the mapper implementation.</typeparam>
    /// <param name="builder">The authentication builder.</param>
    /// <param name="providerName">The name of the provider this mapper handles.</param>
    /// <returns>The authentication builder.</returns>
    public static IVKBlockBuilder<AuthenticationBlock> AddOAuthMapper<TMapper>(this IVKBlockBuilder<AuthenticationBlock> builder, string providerName)
        where TMapper : class, IOAuthClaimsMapper
    {
        builder.Services.AddKeyedScoped<IOAuthClaimsMapper, TMapper>(providerName);
        return builder;
    }

    #endregion

    #region Private Methods

    private static void AddInMemoryCleanupProvider<TService, TImplementation>(this IServiceCollection services, ServiceLifetime lifetime)
        where TService : class
        where TImplementation : class, TService, IInMemoryCacheCleanup
    {
        // 1. Register the concrete implementation as itself
        services.TryAdd(new ServiceDescriptor(typeof(TImplementation), typeof(TImplementation), lifetime));

        // 2. Register the service interface as a factory resolving the concrete implementation
        services.TryAdd(new ServiceDescriptor(typeof(TService), sp => sp.GetRequiredService<TImplementation>(), lifetime));

        // 3. Register the cleanup interface as a factory resolving the concrete implementation
        services.TryAddEnumerable(ServiceDescriptor.Describe(typeof(IInMemoryCacheCleanup), sp => sp.GetRequiredService<TImplementation>(), lifetime));

        // 4. Ensure the background service is registered once
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, InMemoryCleanupBackgroundService>());
    }

    #endregion
}

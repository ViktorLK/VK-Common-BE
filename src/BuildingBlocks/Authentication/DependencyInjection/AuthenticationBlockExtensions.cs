using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Abstractions;
using VK.Blocks.Authentication.ApiKeys;
using VK.Blocks.Authentication.Claims;
using VK.Blocks.Authentication.Factory;
using VK.Blocks.Authentication.OAuth.Mappers;
using VK.Blocks.Authentication.Options;
using VK.Blocks.Authentication.Security;

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
    /// <param name="services">The service collection to add the authentication services to.</param>
    /// <param name="configuration">The configuration instance for reading authentication options.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddVKAuthenticationBlock(this IServiceCollection services, IConfiguration configuration)
    {
        var authOptions = new VKAuthenticationOptions();
        configuration.GetSection(VKAuthenticationOptions.SectionName).Bind(authOptions);

        // Register robust configuration validation
        services.AddSingleton<IValidateOptions<VKAuthenticationOptions>, VKAuthenticationOptionsValidator>();
        services.AddOptions<VKAuthenticationOptions>()
            .Bind(configuration.GetSection(VKAuthenticationOptions.SectionName))
            .ValidateOnStart();

        // Skip configuration if authentication block is disabled
        if (!authOptions.Enabled)
        {
            return services;
        }

        // 1. Core Authentication Setup (JWT + API Key schemes)
        var authBuilder = services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = authOptions.DefaultScheme;
            options.DefaultChallengeScheme = authOptions.DefaultScheme;
        });

        // 2. JWT Configuration
        if (authOptions.Jwt != null && !string.IsNullOrEmpty(authOptions.Jwt.SecretKey))
        {
            authBuilder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = TokenValidationParametersFactory.Create(authOptions.Jwt);
                options.Events = Validation.JwtBearerEventsFactory.CreateEvents();
            });
        }

        // 3. API Key Configuration
        authBuilder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(authOptions.ApiKeySchemeName, options =>
        {
        });

        services.AddScoped<ApiKeyValidator>();

        // 4. Claims Transformation
        services.AddHttpContextAccessor();
        services.AddTransient<IClaimsTransformation, VKClaimsTransformer>();

        // 5. Providers and Services Registration
        services.AddScoped<ITokenBlacklist, DistributedCacheTokenBlacklist>();
        services.AddScoped<IApiKeyBlacklist, DistributedCacheApiKeyBlacklist>();
        services.AddScoped<VK.Blocks.Authentication.Abstractions.IAuthenticationService, VK.Blocks.Authentication.Services.JwtAuthenticationService>();
        services.AddScoped<ITokenRevocationService, TokenRevocationService>();

        services.AddScoped<IRefreshTokenValidator, DistributedRefreshTokenValidator>();
        services.AddScoped<IApiKeyRateLimiter, DistributedCacheApiKeyRateLimiter>();

        // 6. Authorization Policies Registration
        services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, VK.Blocks.Authentication.Authorization.ScopeAuthorizationHandler>();

        // Register OAuth claims mappers using keyed services
        services.AddKeyedScoped<IOAuthClaimsMapper, AzureB2CClaimsMapper>("AzureB2C");
        services.AddKeyedScoped<IOAuthClaimsMapper, GoogleClaimsMapper>("Google");
        services.AddKeyedScoped<IOAuthClaimsMapper, GitHubClaimsMapper>("GitHub");


        return services;
    }

    #endregion
}

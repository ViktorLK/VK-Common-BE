using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Common.Internal;
using VK.Blocks.Authentication.Diagnostics.Internal;
using VK.Blocks.Authentication.Generated;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.DependencyInjection.Internal;

/// <summary>
/// Internal registration logic for the core Authentication block.
/// </summary>
internal static class AuthenticationBlockRegistration
{
    internal static IVKAuthenticationBuilder Register(
        IServiceCollection services,
        IConfiguration configuration,
        Func<VKAuthenticationOptions, VKAuthenticationOptions>? configure = null)
    {
        // 1. Prerequisites & Idempotency Check (Smart Check)
        // Rule 13: This handles both self-idempotency and recursive dependency validation.
        if (services.IsVKBlockRegistered<VKAuthenticationBlock>())
        {
            return new AuthenticationBlockBuilder(services, configuration, null!);
        }

        // 2. Options Registration
        // Rule 15: Bind options before marker registration
        // ADR-016: Use functional transformation to support immutable options
        VKAuthenticationOptions vkAuthOptions = services.AddVKBlockOptions<VKAuthenticationOptions>(configuration, configure);

        // 3. Success Commit (Marker)
        // Rule 13: Register marker immediately after options but before feature-gate early return
        services.AddVKBlockMarker<VKAuthenticationBlock>();

        // 4. Options Validation (Mandatory for config safety)
        services.TryAddEnumerableSingleton<IValidateOptions<VKAuthenticationOptions>, AuthenticationOptionsValidator>();

        // 5. Static Diagnostic Metadata (Always visible to diagnostics)
        services.TryAddEnumerableSingleton<IVKSecurityMetadataProvider, AuthenticationMetadataProvider>();

        // 6. Early Return - Rule 13: Enabled check AFTER marker
        if (!vkAuthOptions.Enabled)
        {
            return new AuthenticationBlockBuilder(services, configuration, null!);
        }

        // 7. Core Infrastructure
        // Must register BEFORE framework to win over NoopClaimsTransformation in AddAuthentication()
        services.TryAddTransient<IClaimsTransformation, ClaimsTransformer>();

        // 8. Framework Integration
        // This registers AddAuthentication and sets up the builder properly
        AuthenticationBuilder authBuilder = AddCoreAuthenticationFramework(services, vkAuthOptions);
        var builder = new AuthenticationBlockBuilder(services, configuration, authBuilder);

        // 9. Auto-Discovery
        services.AddGeneratedClaimsProviders();

        return builder;
    }

    private static AuthenticationBuilder AddCoreAuthenticationFramework(
        IServiceCollection services,
        VKAuthenticationOptions authOptions)
    {
        services.AddHttpContextAccessor();

        return services.AddAuthentication(authSetupOptions =>
        {
            authSetupOptions.DefaultAuthenticateScheme = authOptions.DefaultScheme;
            authSetupOptions.DefaultChallengeScheme = authOptions.DefaultScheme;
        });
    }
}

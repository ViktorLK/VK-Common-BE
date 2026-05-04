using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Authorization.Diagnostics.Internal;
using VK.Blocks.Authorization.Generated;
using VK.Blocks.Core;


namespace VK.Blocks.Authorization.DependencyInjection.Internal;

/// <summary>
/// Principal registration logic for the Authorization building block.
/// Following Rule 18.2 execution sequence.
/// </summary>
internal static class AuthorizationBlockRegistration
{
    internal static IVKAuthorizationBuilder Register(
        IServiceCollection services,
        IConfiguration configuration,
        Func<VKAuthorizationOptions, VKAuthorizationOptions>? transform = null)
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(configuration);

        // 1. Check-Self & Check-Prerequisite
        // Following Rule 13, this smart check handles both idempotency and recursive dependency validation.
        if (services.IsVKBlockRegistered<VKAuthorizationBlock>())
        {
            return new AuthorizationBlockBuilder(services, configuration);
        }

        // 2. Options Registration
        // ADR-016: Functional transformation variant
        VKAuthorizationOptions options = services.AddVKBlockOptions<VKAuthorizationOptions>(configuration, transform);

        // 4. Mark-Self (MUST be called BEFORE early exit)
        services.AddVKBlockMarker<VKAuthorizationBlock>();

        // 5. Options Validation
        services.TryAddEnumerableSingleton<IValidateOptions<VKAuthorizationOptions>, AuthorizationOptionsValidator>();

        // 6. Diagnostics & Metadata
        services.TryAddEnumerableSingleton<IVKSecurityMetadataProvider, AuthorizationMetadataProvider>();

        // ASP.NET Core base services
        services.AddAuthorization();
        services.TryAddEnumerableSingleton<IConfigureOptions<AuthorizationOptions>, AuthorizationPolicyProvider>();

        // 7. Foundation services
        services.TryAddSingleton<IVKSyncStateStore, VKNoOpSyncStateStore>();

        // 8. Register custom handlers discovered by Source Generator
        services.AddGeneratedAuthorizationHandlers();

        var builder = new AuthorizationBlockBuilder(services, configuration);

        // 9. Feature Toggle
        if (!options.Enabled)
        {
            return builder;
        }

        return builder;
    }
}

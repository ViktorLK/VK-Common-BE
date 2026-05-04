using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.TenantIsolation.Internal;

/// <summary>
/// Provides internal registration logic for the Tenant Isolation authorization feature.
/// </summary>
internal static class TenantIsolationRegistration
{
    /// <summary>
    /// Registers the Tenant Isolation authorization feature.
    /// </summary>
    internal static IVKAuthorizationBuilder Register(
        IVKAuthorizationBuilder builder,
        Func<VKTenantIsolationOptions, VKTenantIsolationOptions>? transform = null)
    {
        // 1. Check-Self (Rule 13 & 18.2)
        if (builder.Services.IsVKBlockRegistered<TenantIsolationFeature>())
        {
            return builder;
        }

        var options = builder.Services.AddVKBlockOptions<VKTenantIsolationOptions>(builder.Configuration, transform);

        // 3. Mark-Self (Rule 18.2 - MUST be before early exit)
        builder.Services.AddVKBlockMarker<TenantIsolationFeature>();

        // 4. Options Validation (Rule 18.2)
        builder.Services.TryAddEnumerableSingleton<IValidateOptions<VKTenantIsolationOptions>, TenantIsolationOptionsValidator>();

        if (!options.Enabled)
        {
            return builder;
        }

        var services = builder.Services;

        services.TryAddScoped<IVKUserTenantProvider, DefaultUserTenantProvider>();
        services.TryAddScoped<TenantAuthorizationHandler>();
        services.TryAddEnumerableScopedForwarding<IAuthorizationHandler, TenantAuthorizationHandler>();
        services.TryAddScopedForwarding<IVKTenantEvaluator, TenantAuthorizationHandler>();

        return builder;
    }
}

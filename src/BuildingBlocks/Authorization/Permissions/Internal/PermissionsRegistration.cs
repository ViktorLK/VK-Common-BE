using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.Permissions.Internal;

/// <summary>
/// Provides internal registration logic for the Permissions authorization feature.
/// </summary>
internal static class PermissionsRegistration
{
    /// <summary>
    /// Registers the Permissions authorization feature.
    /// </summary>
    internal static IVKAuthorizationBuilder Register(
        IVKAuthorizationBuilder builder,
        Func<VKPermissionOptions, VKPermissionOptions>? transform = null)
    {
        // 1. Check-Self (AP.02 & 18.2)
        if (builder.Services.IsVKBlockRegistered<PermissionsFeature>())
        {
            return builder;
        }

        var options = builder.Services.AddVKBlockOptions<VKPermissionOptions>(builder.Configuration, transform);

        // 3. Mark-Self (BB.03.2 - MUST be before early exit)
        builder.Services.AddVKBlockMarker<PermissionsFeature>();

        // 4. Options Validation (BB.03.2)
        builder.Services.TryAddEnumerableSingleton<IValidateOptions<VKPermissionOptions>, PermissionOptionsValidator>();

        if (!options.Enabled)
        {
            return builder;
        }

        var services = builder.Services;

        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPermissionProvider, DefaultPermissionProvider>());
        services.TryAddScoped<PermissionHandler>();
        services.TryAddEnumerableScopedForwarding<IAuthorizationHandler, PermissionHandler>();
        services.TryAddScopedForwarding<IVKPermissionEvaluator, PermissionHandler>();

        return builder;
    }
}



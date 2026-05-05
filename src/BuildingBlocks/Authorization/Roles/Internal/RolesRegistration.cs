using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.Roles.Internal;

/// <summary>
/// Provides internal registration logic for the Roles authorization feature.
/// </summary>
internal static class RolesRegistration
{
    /// <summary>
    /// Registers the Roles authorization feature.
    /// </summary>
    internal static IVKAuthorizationBuilder Register(
        IVKAuthorizationBuilder builder,
        Func<VKRoleOptions, VKRoleOptions>? transform = null)
    {
        // 1. Check-Self (Rule 13 & 18.2)
        if (builder.Services.IsVKBlockRegistered<RolesFeature>())
        {
            return builder;
        }

        var options = builder.Services.AddVKBlockOptions<VKRoleOptions>(builder.Configuration, transform);

        // 3. Mark-Self (Rule 18.2 - MUST be before early exit)
        builder.Services.AddVKBlockMarker<RolesFeature>();

        // 4. Options Validation (Rule 18.2)
        builder.Services.TryAddEnumerableSingleton<IValidateOptions<VKRoleOptions>, RoleOptionsValidator>();

        if (!options.Enabled)
        {
            return builder;
        }

        var services = builder.Services;

        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKRoleProvider, DefaultRoleProvider>());
        services.TryAddScoped<RoleHandler>();
        services.TryAddEnumerableScopedForwarding<IAuthorizationHandler, RoleHandler>();
        services.TryAddScopedForwarding<IVKRoleEvaluator, RoleHandler>();

        return builder;
    }
}

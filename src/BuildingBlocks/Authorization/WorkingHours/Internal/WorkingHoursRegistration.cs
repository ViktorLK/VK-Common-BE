using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.WorkingHours.Internal;

/// <summary>
/// Provides internal registration logic for the Working Hours authorization feature.
/// </summary>
internal static class WorkingHoursRegistration
{
    /// <summary>
    /// Registers the Working Hours authorization feature.
    /// </summary>
    internal static IVKAuthorizationBuilder Register(
        IVKAuthorizationBuilder builder,
        Func<VKWorkingHoursOptions, VKWorkingHoursOptions>? transform = null)
    {
        // 1. Check-Self (Rule 13 & 18.2)
        if (builder.Services.IsVKBlockRegistered<WorkingHoursFeature>())
        {
            return builder;
        }

        var options = builder.Services.AddVKBlockOptions<VKWorkingHoursOptions>(builder.Configuration, transform);

        // 3. Mark-Self (Rule 18.2 - MUST be before early exit)
        builder.Services.AddVKBlockMarker<WorkingHoursFeature>();

        // 4. Options Validation (Rule 18.2)
        builder.Services.TryAddEnumerableSingleton<IValidateOptions<VKWorkingHoursOptions>, WorkingHoursOptionsValidator>();

        if (!options.Enabled)
        {
            return builder;
        }

        var services = builder.Services;

        services.TryAddScoped<IVKWorkingHoursProvider, DefaultWorkingHoursProvider>();
        services.TryAddScoped<WorkingHoursAuthorizationHandler>();
        services.TryAddEnumerableScopedForwarding<IAuthorizationHandler, WorkingHoursAuthorizationHandler>();
        services.TryAddScopedForwarding<IVKWorkingHoursEvaluator, WorkingHoursAuthorizationHandler>();

        return builder;
    }
}

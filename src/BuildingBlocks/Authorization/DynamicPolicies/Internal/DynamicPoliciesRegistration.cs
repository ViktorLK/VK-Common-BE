using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.DynamicPolicies.Internal;

/// <summary>
/// Provides internal registration logic for the Dynamic Policies authorization feature.
/// </summary>
internal static class DynamicPoliciesRegistration
{
    /// <summary>
    /// Registers the Dynamic Policies authorization feature.
    /// </summary>
    internal static IVKAuthorizationBuilder Register(
        IVKAuthorizationBuilder builder,
        Func<VKDynamicPoliciesOptions, VKDynamicPoliciesOptions>? transform = null)
    {
        // 1. Check-Self (AP.02 & 18.2)
        if (builder.Services.IsVKBlockRegistered<DynamicPoliciesFeature>())
        {
            return builder;
        }

        var options = builder.Services.AddVKBlockOptions<VKDynamicPoliciesOptions>(builder.Configuration, transform);

        // 3. Mark-Self (BB.03.2 - MUST be before early exit)
        builder.Services.AddVKBlockMarker<DynamicPoliciesFeature>();

        if (!options.Enabled)
        {
            return builder;
        }

        var services = builder.Services;

        services.TryAddScoped<IVKDynamicPoliciesProvider, DefaultDynamicPoliciesProvider>();
        services.TryAddScoped<IVKDynamicPoliciesEvaluator, DefaultDynamicPoliciesEvaluator>();
        services.TryAddScoped<DynamicRequirementHandler>();
        services.TryAddEnumerableScopedForwarding<IAuthorizationHandler, DynamicRequirementHandler>();

        return builder;
    }
}



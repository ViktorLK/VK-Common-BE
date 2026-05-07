using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.MinimumRank.Internal;

/// <summary>
/// Provides internal registration logic for the Minimum Rank authorization feature.
/// </summary>
internal static class MinimumRankRegistration
{
    /// <summary>
    /// Registers the Minimum Rank authorization feature.
    /// </summary>
    internal static IVKAuthorizationBuilder Register(
        IVKAuthorizationBuilder builder,
        Func<VKMinimumRankOptions, VKMinimumRankOptions>? transform = null)
    {
        // 1. Check-Self (AP.02 & 18.2)
        if (builder.Services.IsVKBlockRegistered<MinimumRankFeature>())
        {
            return builder;
        }

        var options = builder.Services.AddVKBlockOptions<VKMinimumRankOptions>(builder.Configuration, transform);

        // 3. Mark-Self (BB.03.2 - MUST be before early exit)
        builder.Services.AddVKBlockMarker<MinimumRankFeature>();

        // 4. Options Validation (BB.03.2)
        builder.Services.TryAddEnumerableSingleton<IValidateOptions<VKMinimumRankOptions>, MinimumRankOptionsValidator>();

        if (!options.Enabled)
        {
            return builder;
        }

        var services = builder.Services;

        services.TryAddScoped<IVKRankProvider, DefaultRankProvider>();
        services.TryAddScoped<MinimumRankAuthorizationHandler>();
        services.TryAddEnumerableScopedForwarding<IAuthorizationHandler, MinimumRankAuthorizationHandler>();
        services.TryAddScopedForwarding<IVKMinimumRankEvaluator, MinimumRankAuthorizationHandler>();

        return builder;
    }
}



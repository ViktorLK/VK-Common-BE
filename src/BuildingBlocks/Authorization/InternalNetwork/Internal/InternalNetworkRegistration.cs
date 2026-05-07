using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.InternalNetwork.Internal;

/// <summary>
/// Provides internal registration logic for the Internal Network authorization feature.
/// </summary>
internal static class InternalNetworkRegistration
{
    /// <summary>
    /// Registers the Internal Network authorization feature.
    /// </summary>
    internal static IVKAuthorizationBuilder Register(
        IVKAuthorizationBuilder builder,
        Func<VKInternalNetworkOptions, VKInternalNetworkOptions>? transform = null)
    {
        // 1. Check-Self (AP.02 & 18.2)
        if (builder.Services.IsVKBlockRegistered<InternalNetworkFeature>())
        {
            return builder;
        }

        var options = builder.Services.AddVKBlockOptions<VKInternalNetworkOptions>(builder.Configuration, transform);

        // 3. Mark-Self (BB.03.2 - MUST be before early exit)
        builder.Services.AddVKBlockMarker<InternalNetworkFeature>();

        // 4. Options Validation (BB.03.2)
        builder.Services.TryAddEnumerableSingleton<IValidateOptions<VKInternalNetworkOptions>, InternalNetworkOptionsValidator>();

        if (!options.Enabled)
        {
            return builder;
        }

        var services = builder.Services;

        services.TryAddScoped<IVKIpAddressProvider, DefaultIpAddressProvider>();
        services.TryAddScoped<InternalNetworkAuthorizationHandler>();
        services.TryAddEnumerableScopedForwarding<IAuthorizationHandler, InternalNetworkAuthorizationHandler>();
        services.TryAddScopedForwarding<IVKInternalNetworkEvaluator, InternalNetworkAuthorizationHandler>();

        return builder;
    }
}



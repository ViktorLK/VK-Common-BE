using System;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.Entitlements.Internal;

/// <summary>
/// Provides internal registration logic for the Entitlements authorization feature.
/// </summary>
internal static class EntitlementsRegistration
{
    /// <summary>
    /// Registers the Entitlements authorization feature.
    /// </summary>
    internal static IVKAuthorizationBuilder Register(
        IVKAuthorizationBuilder builder,
        Func<VKEntitlementsOptions, VKEntitlementsOptions>? transform = null)
    {
        // 1. Check-Self (Rule 13 & 18.2)
        if (builder.Services.IsVKBlockRegistered<EntitlementsFeature>())
        {
            return builder;
        }

        var options = builder.Services.AddVKBlockOptions<VKEntitlementsOptions>(builder.Configuration, transform);

        // 3. Mark-Self (Rule 18.2 - MUST be before early exit)
        builder.Services.AddVKBlockMarker<EntitlementsFeature>();

        if (!options.Enabled)
        {
            return builder;
        }

        // Note: The actual Handler (TenantFeatureAuthorizationHandler) is registered 
        // in the MultiTenancy.Authorization integration block.
        // This registration only ensures the feature is marked as active in the core Authorization block.

        return builder;
    }
}

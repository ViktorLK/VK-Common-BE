using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core.Guids.Internal;
using VK.Blocks.Core.Identity.Internal;
using VK.Blocks.Core.Serialization.Internal;

namespace VK.Blocks.Core.DependencyInjection.Internal;

/// <summary>
/// Principal registration logic for the Core building block.
/// Following Rule 18.2 execution sequence.
/// </summary>
internal static class CoreBlockRegistration
{
    internal static IServiceCollection Register(IServiceCollection services)
    {
        // 1. Check-Self & Check-Prerequisite
        // Rule 13: This smart check handles idempotency.
        if (services.IsVKBlockRegistered<VKCoreBlock>())
        {
            return services;
        }

        // 2. Fundamental services (if any are global)
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<IVKGuidGenerator, SequentialGuidGenerator>();
        services.TryAddSingleton<IVKJsonSerializer, SystemTextJsonSerializer>();
        services.TryAddSingleton<IVKUserContext, NullUserContext>();

        // 3. Mark-Self (Success Commit)
        // Rule 13: Register marker immediately to enable dependency resolution.
        return services.AddVKBlockMarker<VKCoreBlock>();
    }
}

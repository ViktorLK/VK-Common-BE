using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core.Guids.Internal;
using VK.Blocks.Core.Identity.Internal;
using VK.Blocks.Core.Serialization.Internal;
using VK.Blocks.Core.Utilities;

namespace VK.Blocks.Core.DependencyInjection.Internal;

/// <summary>
/// Principal registration logic for the Core building block.
/// Following BB.03.2 execution sequence.
/// </summary>
internal static class CoreBlockRegistration
{
    internal static IServiceCollection Register(IServiceCollection services)
    {
        // 1. Check-Self & Check-Prerequisite
        // AP.02: This smart check handles idempotency.
        if (services.IsVKBlockRegistered<VKCoreBlock>())
        {
            return services;
        }

        // 2. Fundamental services (if any are global)
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<IVKGuidGenerator, SequentialGuidGenerator>();
        services.TryAddSingleton<IVKJsonSerializer, SystemTextJsonSerializer>();
        services.TryAddSingleton<IVKEnvironmentProvider, VKDefaultEnvironmentProvider>();
        services.TryAddSingleton<IVKUserContext, NullUserContext>();

        // 3. Mark-Self (Success Commit)
        // AP.02: Register marker immediately to enable dependency resolution.
        return services.AddVKBlockMarker<VKCoreBlock>();
    }
}



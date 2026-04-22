using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core.Context.Internal;
using VK.Blocks.Core.Guids.Internal;
using VK.Blocks.Core.Json.Internal;

namespace VK.Blocks.Core;

/// <summary>
/// Extension methods for registering the core building block.
/// </summary>
public static class VKCoreBlockExtensions
{
    /// <summary>
    /// Adds the core building block services and marker.
    /// This should be called before other building blocks are registered.
    /// </summary>
    public static IServiceCollection AddVKCoreBlock(this IServiceCollection services, IConfiguration _)
    {
        // 0. Idempotency Check
        if (services.IsVKBlockRegistered<VKCoreBlock>())
        {
            return services;
        }

        // 1. Fundamental services (if any are global)
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<IVKGuidGenerator, SequentialGuidGenerator>();
        services.TryAddSingleton<IVKJsonSerializer, SystemTextJsonSerializer>();
        services.TryAddSingleton<IVKUserContext, NullUserContext>();

        // 2. Mark-Self (Success Commit)
        return services.AddVKBlockMarker<VKCoreBlock>();
    }
}

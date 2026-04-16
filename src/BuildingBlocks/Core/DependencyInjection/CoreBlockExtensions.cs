using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core.Abstractions;
using VK.Blocks.Core.Abstractions.Internal;
using VK.Blocks.Core.Internal;

namespace VK.Blocks.Core.DependencyInjection;

/// <summary>
/// Extension methods for registering the core building block.
/// </summary>
public static class CoreBlockExtensions
{
    /// <summary>
    /// Adds the core building block services and marker.
    /// This should be called before other building blocks are registered.
    /// </summary>
    public static IServiceCollection AddVKCoreBlock(this IServiceCollection services, IConfiguration configuration)
    {
        // 0. Idempotency Check
        if (services.IsVKBlockRegistered<CoreBlock>())
        {
            return services;
        }

        // 1. Fundamental services (if any are global)
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<IGuidGenerator, SequentialGuidGenerator>();
        services.TryAddSingleton<IJsonSerializer, SystemTextJsonSerializer>();
        services.TryAddSingleton<IUserContext, NullUserContext>();

        // 2. Mark-Self (Success Commit)
        return services.AddVKBlockMarker<CoreBlock>();
    }
}

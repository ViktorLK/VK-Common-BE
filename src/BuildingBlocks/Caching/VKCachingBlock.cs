using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Caching.ObjectCache.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.Caching;

/// <summary>
/// Marker type for the Caching building block.
/// Follows BB.02, BB.03, AP.02.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)])]
public sealed partial class VKCachingBlock
{
    static partial void RegisterBlockCustom(IVKCachingBuilder builder)
    {
        var services = builder.Services;

        // Retrieve option instance
        var defaults = services.GetVKServiceInstance<VKCachingOptions>();

        // 1. Register HybridCache as the core engine
        services.AddHybridCache(options =>
        {
            if (defaults is not null)
            {
                options.DefaultEntryOptions = new HybridCacheEntryOptions
                {
                    Expiration = defaults.DefaultExpiration,
                    LocalCacheExpiration = defaults.DefaultLocalExpiration
                };

                if (defaults.MaximumPayloadBytes.HasValue)
                {
                    options.MaximumPayloadBytes = defaults.MaximumPayloadBytes.Value;
                }
            }
        });

        // 2. Register key builder and cache block facade
        services.TryAddSingleton<IVKCacheKeyBuilder, DefaultCacheKeyBuilder>();
        services.TryAddSingleton<IVKCacheBlock, DefaultCacheBlock>();
    }
}

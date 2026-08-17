using VK.Blocks.Caching.ObjectCache;
using VK.Blocks.Caching.ObjectCache.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using VK.Blocks.Caching.ObjectCache.Internal.Providers.Hybrid;
using VK.Blocks.Caching.ObjectCache.Internal.Providers.Memory;
using VK.Blocks.Caching.ObjectCache.Internal.Providers.Redis;
using VK.Blocks.Caching.ObjectCache.Internal.Providers.SqlServer;
using VK.Blocks.Core;

namespace VK.Blocks.Caching;

[VKFeature(typeof(VKCachingBlock), OptionsType = typeof(VKObjectCacheOptions))]
internal sealed partial class ObjectCacheFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKObjectCacheOptions options)
    {
        services.TryAddSingleton<ICacheSerializer, DefaultCacheSerializer>();
        services.TryAddSingleton<ICacheKeyBuilder, DefaultCacheKeyBuilder>();
        services.TryAddSingleton<IVKCacheBlock, DefaultCacheBlock>();

        switch (options.Provider)
        {
            case CacheProviderType.Redis:
                services.TryAddSingleton<ICacheProvider, RedisCacheProvider>();
                break;
            case CacheProviderType.Hybrid:
                services.TryAddSingleton<BasicCacheProvider>();
                services.TryAddSingleton<RedisCacheProvider>();
                services.TryAddSingleton<ICacheProvider>(sp =>
                    new HybridCacheProvider(
                        sp.GetRequiredService<BasicCacheProvider>(),
                        sp.GetRequiredService<RedisCacheProvider>(),
                        Options.Create(options.Hybrid),
                        sp.GetService<IConnectionMultiplexer>()
                    ));
                break;
            case CacheProviderType.SqlServer:
                services.TryAddSingleton<ICacheProvider, SqlServerCacheProvider>();
                break;
            case CacheProviderType.Memory:
            default:
                services.TryAddSingleton<ICacheProvider, BasicCacheProvider>();
                break;
        }
    }

    static partial void ValidateFeatureCustom(VKObjectCacheOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}

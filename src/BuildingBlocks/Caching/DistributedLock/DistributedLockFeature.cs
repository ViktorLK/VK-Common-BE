using VK.Blocks.Caching.DistributedLock;
using VK.Blocks.Caching.DistributedLock.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;
using VK.Blocks.Caching.DistributedLock.Internal.Providers.Memory;
using VK.Blocks.Caching.DistributedLock.Internal.Providers.Redis;
using VK.Blocks.Core;

namespace VK.Blocks.Caching;

[VKFeature(typeof(VKCachingBlock), OptionsType = typeof(VKDistributedLockOptions))]
internal sealed partial class DistributedLockFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKDistributedLockOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKDistributedLockProvider>(sp =>
        {
            var redis = sp.GetService<IConnectionMultiplexer>();
            if (redis is not null)
            {
                return new RedisDistributedLockProvider(redis, sp.GetRequiredService<IVKGuidGenerator>());
            }
            return new MemoryDistributedLockProvider();
        });
    }

    static partial void ValidateFeatureCustom(VKDistributedLockOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}

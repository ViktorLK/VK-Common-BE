using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Caching.Resilience.Avalanche;
using VK.Blocks.Caching.Resilience.Breakdown;
using VK.Blocks.Caching.Resilience.Penetration;
using VK.Blocks.Core;

namespace VK.Blocks.Caching;

[VKFeature(typeof(VKCachingBlock), OptionsType = typeof(VKResilienceOptions))]
internal sealed partial class ResilienceFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKResilienceOptions options)
    {
        _ = options;
        services.TryAddSingleton<IJitterExpiryStrategy, JitterExpiryStrategy>();
        services.TryAddSingleton<ILockBasedGuard, LockBasedGuard>();
        services.TryAddSingleton<INullValueGuard, NullValueGuard>();
    }

    static partial void ValidateFeatureCustom(VKResilienceOptions options, System.Collections.Generic.List<string> failures)
    {
        if (options.MaxJitterRatio < 0.0 || options.MaxJitterRatio > 1.0)
        {
            failures.Add("MaxJitterRatio must be between 0.0 and 1.0.");
        }
    }
}

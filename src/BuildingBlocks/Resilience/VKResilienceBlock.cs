using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
using VK.Blocks.Resilience.Bulkhead.Internal;
using VK.Blocks.Resilience.Caching.Internal;
using VK.Blocks.Resilience.CircuitBreaker.Internal;
using VK.Blocks.Resilience.Common.Internal;
using VK.Blocks.Resilience.Fallback.Internal;
using VK.Blocks.Resilience.RateLimiting.Internal;
using VK.Blocks.Resilience.Retry.Internal;
using VK.Blocks.Resilience.Timeout.Internal;

namespace VK.Blocks.Resilience;

/// <summary>
/// A marker type for the VK.Blocks.Resilience building block.
/// Follows [AP.01], [AP.02], [BB.02], [BB.03].
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)])]
public sealed partial class VKResilienceBlock
{
    static partial void RegisterBlockCustom(IVKResilienceBuilder builder)
    {
        var services = builder.Services;

        // Default options registrations
        services.TryAddSingleton(new VKRetryOptions());
        services.TryAddSingleton(new VKTimeoutOptions());
        services.TryAddSingleton(new VKCircuitBreakerOptions());
        services.TryAddSingleton(new VKRateLimiterOptions());
        services.TryAddSingleton(new VKTokenBucketOptions());
        services.TryAddSingleton(new VKBulkheadOptions());
        services.TryAddSingleton(new VKFallbackOptions());

        // Strategy Executors & Limiters
        services.TryAddSingleton<IVKRetryExecutor, LocalRetryExecutor>();
        services.TryAddSingleton<IVKTimeoutExecutor, LocalTimeoutExecutor>();
        services.TryAddSingleton<IVKCircuitBreaker, LocalCircuitBreaker>();
        services.TryAddSingleton<IVKRateLimiter, LocalRateLimiter>();
        services.TryAddSingleton<IVKTokenBucketLimiter, LocalTokenBucketLimiter>();
        services.TryAddSingleton<IVKBulkhead, LocalBulkhead>();
        services.TryAddSingleton<IVKFallbackHandler, LocalFallbackHandler>();
        services.TryAddSingleton<IVKCacheResilienceProvider, LocalCacheResilienceProvider>();

        // Central Registry
        services.TryAddSingleton<IVKPolicyRegistry, DefaultPolicyRegistry>();

        // Health Checks
        services.TryAddSingleton<VKResilienceHealthCheck>();
    }
}

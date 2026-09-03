using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
using VK.Blocks.Resilience.RateLimiting.Internal;

namespace VK.Blocks.Resilience;

[VKFeature(typeof(VKResilienceBlock), OptionsType = typeof(VKTokenBucketOptions))]
internal sealed partial class TokenBucketFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKTokenBucketOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKTokenBucketLimiter, LocalTokenBucketLimiter>();
    }
}

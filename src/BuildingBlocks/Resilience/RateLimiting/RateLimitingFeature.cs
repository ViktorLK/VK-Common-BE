using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
using VK.Blocks.Resilience.RateLimiting.Internal;

namespace VK.Blocks.Resilience;

[VKFeature(typeof(VKResilienceBlock), OptionsType = typeof(VKRateLimiterOptions))]
internal sealed partial class RateLimitingFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKRateLimiterOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKRateLimiter, LocalRateLimiter>();
    }

    static partial void ValidateFeatureCustom(VKRateLimiterOptions options, System.Collections.Generic.List<string> failures)
    {
        if (options.PermitLimit <= 0)
        {
            failures.Add("PermitLimit must be greater than zero.");
        }

        if (options.Window <= TimeSpan.Zero)
        {
            failures.Add("Window duration must be greater than zero.");
        }
    }
}

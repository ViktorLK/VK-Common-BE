using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
using VK.Blocks.Resilience.CircuitBreaker.Internal;

namespace VK.Blocks.Resilience;

[VKFeature(typeof(VKResilienceBlock), OptionsType = typeof(VKCircuitBreakerOptions))]
internal sealed partial class CircuitBreakerFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKCircuitBreakerOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKCircuitBreaker, LocalCircuitBreaker>();
    }

    static partial void ValidateFeatureCustom(VKCircuitBreakerOptions options, System.Collections.Generic.List<string> failures)
    {
        if (options.FailureThreshold <= 0 || options.FailureThreshold > 1.0)
        {
            failures.Add("FailureThreshold must be between 0 (exclusive) and 1.0 (inclusive).");
        }

        if (options.DurationOfBreak <= TimeSpan.Zero)
        {
            failures.Add("DurationOfBreak must be greater than zero.");
        }

        if (options.MinimumThroughput < 0)
        {
            failures.Add("MinimumThroughput must be non-negative.");
        }
    }
}

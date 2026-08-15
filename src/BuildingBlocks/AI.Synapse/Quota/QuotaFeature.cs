using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI;
using VK.Blocks.AI.Synapse.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse;

[VKFeature(typeof(VKAISynapseBlock), OptionsType = typeof(VKQuotaOptions))]
internal sealed partial class QuotaFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKQuotaOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKAICircuitBreaker, LocalAICircuitBreaker>();
        services.TryAddSingleton<IVKAIRateLimiter, LocalAIRateLimiter>();
        services.TryAddSingleton<IVKAITokenBudgetManager, LocalAITokenBudgetManager>();
        services.TryAddSingleton<IVKAIMetricsCollector, LocalAIMetricsCollector>();
        services.TryAddSingleton<IVKAIProviderTracker, DefaultAIProviderTracker>();
    }

    static partial void ValidateFeatureCustom(VKQuotaOptions options, System.Collections.Generic.List<string> failures)
    {
        if (options.DefaultCircuitBreakerThreshold <= 0)
        {
            failures.Add("DefaultCircuitBreakerThreshold must be greater than zero.");
        }
        if (options.DefaultMaxConcurrency <= 0)
        {
            failures.Add("DefaultMaxConcurrency must be greater than zero.");
        }
    }
}

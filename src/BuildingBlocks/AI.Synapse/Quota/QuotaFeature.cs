using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI;
using VK.Blocks.AI.Synapse.Quota.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse;

[VKFeature(typeof(VKAISynapseBlock), OptionsType = typeof(VKQuotaOptions))]
internal sealed partial class QuotaFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKQuotaOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKAIRateLimiter, DefaultAIRateLimiter>();
        services.TryAddSingleton<IVKAITokenBudgetManager, DefaultAITokenBudgetManager>();
        services.TryAddSingleton<IVKAIMetricsCollector, DefaultAIMetricsCollector>();
        services.TryAddSingleton<IVKAIProviderTracker, DefaultAIProviderTracker>();
        services.TryAddSingleton<IVKAICumulativeQuotaStore, InMemoryCumulativeQuotaStore>();
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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Synapse.Routing.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse;

[VKFeature(typeof(VKAISynapseBlock), OptionsType = typeof(VKRoutingOptions))]
internal sealed partial class RoutingFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKRoutingOptions options)
    {
        // [AP.02] Always register observer so post-call cost/usage observation remains active in cost-only gateway mode.
        services.Replace(ServiceDescriptor.Singleton<IVKAIInvocationObserver, DefaultAIInvocationObserver>());

        if (options.Enabled)
        {
            services.TryAddSingleton<IVKAIConnectionStore, InMemoryAIConnectionStore>();
            services.TryAddSingleton<IVKAIRouter, DefaultAIRouter>();
            services.TryAddSingleton<IVKAIProviderPool, DefaultAIProviderPool>();
            services.TryAddSingleton<IVKAIAffinityPinStore, InMemoryAffinityPinStore>();

            // Override VK.Blocks.AI NoOp default when routing is enabled [AP.02]
            services.Replace(ServiceDescriptor.Singleton<IVKAIRoutingPolicy, DefaultAIRoutingPolicy>());
        }
    }

    static partial void ValidateFeatureCustom(VKRoutingOptions options, System.Collections.Generic.List<string> failures)
    {
        if (!options.Enabled)
        {
            return;
        }

        if (options.MaxFallbackAttempts <= 0)
        {
            failures.Add("MaxFallbackAttempts must be greater than zero.");
        }

        if (options.Profiles is { Count: > 0 })
        {
            foreach (var profile in options.Profiles)
            {
                if (profile.CandidateConnectionIds is null || profile.CandidateConnectionIds.Count == 0)
                {
                    failures.Add($"Profile '{profile.Name}' must have at least one candidate connection ID.");
                }
            }
        }
    }
}

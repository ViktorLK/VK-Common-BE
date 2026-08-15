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
        _ = options;
        services.TryAddSingleton<IVKAIConnectionStore, InMemoryAIConnectionStore>();
        services.TryAddSingleton<IVKAIEngineAccessor, DefaultAIEngineAccessor>();
        services.TryAddSingleton<IVKAIRouter, DefaultAIRouter>();
        services.TryAddSingleton<IVKAIProviderPool, DefaultAIProviderPool>();
        services.TryAddSingleton<IVKAIRouteDispatcher, DefaultAIRouteDispatcher>();
    }

    static partial void ValidateFeatureCustom(VKRoutingOptions options, System.Collections.Generic.List<string> failures)
    {
        if (options.MaxFallbackAttempts <= 0)
        {
            failures.Add("MaxFallbackAttempts must be greater than zero.");
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Routing.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

[VKFeature(typeof(VKAIBlock), OptionsType = typeof(VKRoutingOptions))]
internal sealed partial class RoutingFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKRoutingOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKAIRoutingPolicy, NoOpAIRoutingPolicy>();
        services.TryAddSingleton<IVKAIInvocationObserver, NoOpAIInvocationObserver>();
        services.TryAddSingleton<IVKAICredentialResolver, NoOpAICredentialResolver>();
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKRoutingOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}

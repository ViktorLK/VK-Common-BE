using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Governance.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

[VKFeature(typeof(VKAIBlock), OptionsType = typeof(VKGovernanceOptions))]
internal sealed partial class GovernanceFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKGovernanceOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKAICache, InMemoryAICache>();
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKGovernanceOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}

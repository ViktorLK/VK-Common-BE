using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Synapse.Cost.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse;

[VKFeature(typeof(VKAISynapseBlock), OptionsType = typeof(VKCostOptions))]
internal sealed partial class CostFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKCostOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKAICostCalculator, DefaultAICostCalculator>();
    }

    static partial void ValidateFeatureCustom(VKCostOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}

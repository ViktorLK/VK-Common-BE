using VK.Blocks.AI.Tokenics.Costing.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Token Costing feature marker and registration hub.
/// </summary>
[VKFeature(typeof(global::VK.Blocks.AI.TokenicsFeature), OptionsType = typeof(VKCostingOptions))]
internal sealed partial class CostingFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKCostingOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKTokenCostCalculator, DefaultTokenCostCalculator>();
    }

    /// <summary>Add costing-specific validation logic here</summary>
    // [SG Hook]
    static partial void ValidateFeatureCustom(VKCostingOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}

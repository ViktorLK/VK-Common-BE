using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.AI.Tokenics.Costing.Internal;

/// <summary>
/// Token Costing feature marker and registration hub.
/// </summary>
internal sealed partial class CostingFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKCostingOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKTokenCostCalculator, DefaultTokenCostCalculator>();
    }

    /// <summary>Add costing-specific validation logic here</summary>
    // [SG Hook]
    static partial void ValidateCustom(VKCostingOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}

using VK.Blocks.AI.Tokenics.Budgeting.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Token Budgeting feature marker and registration hub.
/// </summary>
[VKFeature(typeof(global::VK.Blocks.AI.TokenicsFeature), OptionsType = typeof(VKBudgetingOptions), ArgsGenerationMode = VKArgsGenerationMode.Explicit)]
internal sealed partial class BudgetingFeature
{
    /// <summary>Add budgeting services here</summary>
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKBudgetingOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKTokenBudgeter, DefaultTokenBudgeter>();
        services.TryAddSingleton<IVKTokenUsageAggregator, DefaultTokenUsageAggregator>();
    }

    /// <summary>Add budgeting-specific validation logic here</summary>
    // [SG Hook]
    static partial void ValidateFeatureCustom(VKBudgetingOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}

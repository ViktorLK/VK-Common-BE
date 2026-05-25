using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.AI.Tokenics.Budgeting.Internal;

/// <summary>
/// Token Budgeting feature marker and registration hub.
/// </summary>
internal sealed partial class BudgetingFeature
{
    /// <summary>Add budgeting services here</summary>
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKBudgetingOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKTokenBudgeter, DefaultTokenBudgeter>();
        services.TryAddSingleton<IVKTokenUsageAggregator, DefaultTokenUsageAggregator>();
    }

    /// <summary>Add budgeting-specific validation logic here</summary>
    // [SG Hook]
    static partial void ValidateCustom(VKBudgetingOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}

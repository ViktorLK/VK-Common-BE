using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.AI.Vectorics.ReRanking.Internal;

/// <summary>
/// Re-Ranking feature marker and registration hub.
/// </summary>
internal sealed partial class ReRankingFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKReRankingOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKReRanker, NoOpVKReRanker>();
    }

    /// <summary>Add re-ranking-specific validation logic here</summary>
    // [SG Hook]
    static partial void ValidateCustom(VKReRankingOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}

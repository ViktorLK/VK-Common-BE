using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Aggregates all Re-Ranking configuration settings.
/// </summary>
public interface IVKReRankingOptions :
    IVKAIProviderOptions,
    IVKAIResilienceOptions,
    IVKToggleableBlockOptions
{
}

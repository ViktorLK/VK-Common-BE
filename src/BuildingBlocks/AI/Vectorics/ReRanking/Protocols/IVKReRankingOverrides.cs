namespace VK.Blocks.AI;

/// <summary>
/// Defines re-ranking parameters that can be overridden at the request level.
/// </summary>
public interface IVKReRankingOverrides : IVKAIProviderOverrides, IVKAIResilienceOverrides
{
    // Add ReRanking specific overrides here if needed in the future
}

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Defines the fusion strategies available for hybrid search.
/// </summary>
public enum VKFusionStrategy
{
    /// <summary>
    /// Reciprocal Rank Fusion (RRF).
    /// </summary>
    RRF,

    /// <summary>
    /// Weighted Score Fusion.
    /// </summary>
    WeightedScore
}

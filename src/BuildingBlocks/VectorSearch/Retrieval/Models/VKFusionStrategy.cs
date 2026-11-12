namespace VK.Blocks.VectorSearch;

/// <summary>
/// Defines the fusion strategies available for hybrid search.
/// </summary>
public enum VKFusionStrategy : byte
{
    /// <summary>
    /// Reciprocal Rank Fusion (RRF).
    /// </summary>
    RRF = 0,

    /// <summary>
    /// Weighted Score Fusion.
    /// </summary>
    WeightedScore = 1
}

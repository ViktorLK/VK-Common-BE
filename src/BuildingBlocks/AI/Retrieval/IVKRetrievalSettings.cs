namespace VK.Blocks.AI;

/// <summary>
/// Defines retrieval parameters for semantic retrieval operations.
/// </summary>
public interface IVKRetrievalSettings
{
    /// <summary>
    /// Gets the maximum number of results to retrieve.
    /// </summary>
    int? TopK { get; init; }

    /// <summary>
    /// Gets the minimum score threshold for retrieval.
    /// </summary>
    float? MinScore { get; init; }
}

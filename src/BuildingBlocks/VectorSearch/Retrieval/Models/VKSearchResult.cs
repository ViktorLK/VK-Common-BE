namespace VK.Blocks.VectorSearch;

/// <summary>
/// Represents the result of a recall search.
/// </summary>
public sealed record VKSearchResult
{
    /// <summary>
    /// Gets the matched document.
    /// </summary>
    public required VKDocument Document { get; init; }

    /// <summary>
    /// Gets the match score.
    /// </summary>
    public required double Score { get; init; }
}

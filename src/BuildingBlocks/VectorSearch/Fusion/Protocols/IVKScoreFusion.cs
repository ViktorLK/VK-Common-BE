using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Defines the contract for fusing multiple search result lists into a single ranked result list.
/// </summary>
public interface IVKScoreFusion
{
    /// <summary>
    /// Fuses multiple ranked lists of candidates into a single ranked list.
    /// </summary>
    /// <param name="runs">The list of ranked search runs, each run containing candidates in sorted order.</param>
    /// <param name="topK">The maximum number of fused results to return.</param>
    /// <returns>A unified, sorted list of fusion results containing document chunks and fused scores.</returns>
    VKResult<IReadOnlyList<VKSearchResult>> Fuse(
        IReadOnlyList<IReadOnlyList<VKFusionCandidate>> runs,
        int topK);
}

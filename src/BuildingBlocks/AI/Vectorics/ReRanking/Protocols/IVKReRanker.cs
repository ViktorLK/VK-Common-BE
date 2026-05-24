using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Defines the contract for an AI Re-Ranking engine.
/// Re-ranking takes an initial set of results and re-evaluates their relevance to a query.
/// </summary>
public interface IVKReRanker
{
    /// <summary>
    /// Re-ranks a collection of candidate results based on a query.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="candidates">The list of candidate items (text or metadata).</param>
    /// <param name="args">The execution arguments (overrides).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of re-ranked results with scores.</returns>
    Task<VKResult<IReadOnlyList<VKReRankingResult>>> ReRankAsync(
        string query,
        IEnumerable<string> candidates,
        VKReRankingArgs? args = null,
        CancellationToken cancellationToken = default);
}

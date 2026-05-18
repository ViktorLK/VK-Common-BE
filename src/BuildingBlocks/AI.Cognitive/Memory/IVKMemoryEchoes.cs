using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines the interface for storing and retrieving AI memories.
/// </summary>
public interface IVKMemoryEchoes
{
    /// <summary>
    /// Saves a memory entry.
    /// </summary>
    /// <param name="entry">The entry to save.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    Task<VKResult> SaveAsync(VKMemoryEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for memories similar to the query.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="limit">The maximum number of results.</param>
    /// <param name="minScore">The minimum relevance score.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list of search results.</returns>
    Task<VKResult<IEnumerable<VKMemoryQueryResult>>> SearchAsync(
        string query,
        int limit = 5,
        float minScore = 0.7f,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for memories similar to the query using retrieval arguments.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="args">The retrieval arguments.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list of search results.</returns>
    Task<VKResult<IEnumerable<VKMemoryQueryResult>>> SearchAsync(
        string query,
        VKRetrievalArgs? args = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Prunes/removes memories that have decayed below a certain threshold or are older than a specific date.
    /// </summary>
    /// <param name="before">The cutoff date for pruning.</param>
    /// <param name="minImportance">The minimum importance threshold.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the number of pruned entries.</returns>
    Task<VKResult<int>> PruneAsync(
        DateTimeOffset? before = null,
        float minImportance = 0.0f,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a specific memory entry by its ID.
    /// </summary>
    /// <param name="id">The memory ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    Task<VKResult> RemoveAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Re-ranks a set of memory query results based on relevance, importance, and temporal decay.
    /// </summary>
    /// <param name="results">The initial search results.</param>
    /// <param name="query">The original search query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the re-ranked list.</returns>
    Task<VKResult<IEnumerable<VKMemoryQueryResult>>> ReRankAsync(
        IEnumerable<VKMemoryQueryResult> results,
        string query,
        CancellationToken cancellationToken = default);
}

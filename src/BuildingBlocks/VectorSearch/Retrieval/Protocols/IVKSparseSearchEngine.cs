using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Defines the contract for a technology-dependent sparse/BM25 search engine.
/// </summary>
public interface IVKSparseSearchEngine
{
    /// <summary>
    /// Searches the index using BM25/keyword matching.
    /// </summary>
    Task<VKResult<IReadOnlyList<VKSparseSearchResult>>> SearchAsync(
        VKSearchQuery query,
        CancellationToken cancellationToken = default);
}

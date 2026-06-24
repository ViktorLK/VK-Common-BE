using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Defines the contract for a vector search reranker.
/// </summary>
public interface IVKVectorReranker
{
    /// <summary>
    /// Re-ranks candidates based on the search query.
    /// </summary>
    Task<VKResult<IReadOnlyList<VKRerankResult>>> RerankAsync(
        VKRerankRequest request,
        CancellationToken cancellationToken = default);
}

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;
using VK.Blocks.AI.VectorStore.Retrieval.Models;

namespace VK.Blocks.AI.VectorStore.Retrieval.Protocols;

/// <summary>
/// [Advanced RAG] Defines a strategy for executing high-level document retrieval.
/// </summary>
public interface IVKRetrievalStrategy
{
    /// <summary>
    /// Executes the retrieval strategy (e.g., Hybrid Search, Multi-Query) to find relevant document chunks.
    /// </summary>
    Task<VKResult<IEnumerable<VKVectorSearchResult>>> ExecuteAsync(
        string query,
        VKRetrievalArgs? args = null,
        CancellationToken cancellationToken = default);
}

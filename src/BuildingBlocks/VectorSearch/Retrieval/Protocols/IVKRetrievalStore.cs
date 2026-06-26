using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;
using VK.Blocks.VectorStore;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Defines the interface for a retrieval store.
/// </summary>
public interface IVKRetrievalStore
{
    /// <summary>
    /// Upserts a collection of document chunks with their embeddings.
    /// </summary>
    Task<VKResult> UpsertAsync(
        IEnumerable<VKDocumentChunk> chunks,
        IEnumerable<VKVector> embeddings,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for chunks similar to the query embedding.
    /// </summary>
    Task<VKResult<IEnumerable<VKVectorSearchResult>>> SearchAsync(
        VKVector embedding,
        VKVectorSearchArgs? args = null,
        CancellationToken cancellationToken = default);
}

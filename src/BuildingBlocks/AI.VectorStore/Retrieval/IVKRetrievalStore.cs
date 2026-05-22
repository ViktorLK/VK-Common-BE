using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.VectorStore;

/// <summary>
/// Defines the interface for a vector store.
/// </summary>
public interface IVKRetrievalStore
{
    /// <summary>
    /// Upserts a collection of document chunks with their embeddings.
    /// </summary>
    /// <param name="chunks">The chunks to upsert.</param>
    /// <param name="embeddings">The corresponding embeddings.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result.</returns>
    Task<VKResult> UpsertAsync(
        IEnumerable<VKDocumentChunk> chunks,
        IEnumerable<VKEmbeddingsVector> embeddings,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for chunks similar to the query embedding.
    /// </summary>
    /// <param name="embedding">The query embedding.</param>
    /// <param name="args">The search arguments.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The search results.</returns>
    Task<VKResult<IEnumerable<VKVectorSearchResult>>> SearchAsync(
        VKEmbeddingsVector embedding,
        VKRetrievalArgs? args = null,
        CancellationToken cancellationToken = default);
}

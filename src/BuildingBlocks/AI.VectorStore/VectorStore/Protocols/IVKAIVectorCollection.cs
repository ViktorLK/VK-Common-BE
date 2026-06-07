using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.VectorStore.VectorStore.Protocols;

/// <summary>
/// Defines a high-level, generic collection within a vector store.
/// Following the Industrial SDK pattern (MongoDB/Pinecone style).
/// </summary>
/// <typeparam name="T">The type of the document stored with the vector.</typeparam>
public interface IVKAIVectorCollection<T> where T : class
{
    /// <summary>
    /// Gets the name of the collection.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Upserts a document and its vector representation.
    /// </summary>
    Task<VKResult> UpsertAsync(
        string id,
        T document,
        VKEmbeddingsVector vector,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for similar documents based on a query vector.
    /// </summary>
    Task<VKResult<IEnumerable<VKAIVectorRecord<T>>>> SearchAsync(
        VKEmbeddingsVector vector,
        VKAIVectorSearchArgs args,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a document by its identifier.
    /// </summary>
    Task<VKResult> DeleteAsync(string id, string? tenantId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// [Performance] Upserts a batch of documents and their vector representations efficiently.
    /// </summary>
    Task<VKResult> UpsertBatchAsync(
        IEnumerable<(string Id, T Document, VKEmbeddingsVector Vector)> records,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// [Performance] Streams search results for large result sets.
    /// </summary>
    IAsyncEnumerable<VKResult<VKAIVectorRecord<T>>> SearchStreamAsync(
        VKEmbeddingsVector vector,
        VKAIVectorSearchArgs args,
        CancellationToken cancellationToken = default);
}

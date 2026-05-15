using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.VectorStore.Contracts;
using VK.Blocks.Core;

namespace VK.Blocks.AI.VectorStore;

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
        VKEmbeddingVector vector,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for similar documents based on a query vector.
    /// </summary>
    Task<VKResult<IEnumerable<VKAIVectorRecord<T>>>> SearchAsync(
        VKEmbeddingVector vector,
        VKAIVectorSearchArgs args,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a document by its identifier.
    /// </summary>
    Task<VKResult> DeleteAsync(string id, string? tenantId = null, CancellationToken cancellationToken = default);
}

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.VectorStore;

/// <summary>
/// Defines a high-level, generic collection within a vector store.
/// Following the Industrial SDK pattern (MongoDB/Pinecone style).
/// </summary>
/// <typeparam name="T">The type of the document stored with the vector.</typeparam>
public interface IVKVectorCollection<T> where T : class
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
        VKVector vector,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for similar documents based on a query vector.
    /// </summary>
    Task<VKResult<IEnumerable<VKVectorRecord<T>>>> SearchAsync(
        VKVector vector,
        VKVectorSearchArgs args,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a document by its identifier.
    /// </summary>
    Task<VKResult> DeleteAsync(string id, string? tenantId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// [Performance] Upserts a batch of documents and their vector representations efficiently.
    /// </summary>
    Task<VKResult> UpsertBatchAsync(
        IEnumerable<(string Id, T Document, VKVector Vector)> records,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// [Performance] Streams search results for large result sets.
    /// </summary>
    IAsyncEnumerable<VKResult<VKVectorRecord<T>>> SearchStreamAsync(
        VKVector vector,
        VKVectorSearchArgs args,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific document and its vector by ID.
    /// Default implementation returns null to prevent breaking external implementers.
    /// </summary>
    Task<VKResult<VKVectorRecord<T>?>> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any document exists in the collection matching the specified metadata filter.
    /// </summary>
    Task<VKResult<bool>> ExistsAsync(
        VKMetadataFilter filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries the collection for documents matching the specified metadata filter.
    /// </summary>
    Task<VKResult<IEnumerable<VKVectorRecord<T>>>> QueryAsync(
        VKMetadataFilter filter,
        CancellationToken cancellationToken = default);
}


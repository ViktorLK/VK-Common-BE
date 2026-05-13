using VK.Blocks.Core;

namespace VK.Blocks.AI.VectorStore;

/// <summary>
/// Defines the low-level contract for a vector database provider.
/// </summary>
public interface IVKAIVectorDatabase
{
    /// <summary>
    /// Upserts a vector and its metadata.
    /// </summary>
    Task<VKResult> UpsertAsync(
        string id,
        VKEmbeddingVector vector,
        string content,
        VKAIVectorMetadata metadata,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for similar vectors within a specific tenant's scope.
    /// </summary>
    Task<VKResult<IEnumerable<VKAIVectorRecord>>> SearchAsync(
        VKEmbeddingVector vector,
        VKAIVectorSearchArgs args,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a record by ID.
    /// </summary>
    Task<VKResult> DeleteAsync(string tenantId, string id, CancellationToken cancellationToken = default);
}

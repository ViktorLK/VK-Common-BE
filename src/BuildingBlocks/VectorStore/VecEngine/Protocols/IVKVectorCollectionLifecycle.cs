using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.VectorStore;

/// <summary>
/// Exposes lifecycle and administrative operations for dedicated vector stores (Qdrant, Cosmos, Azure AI Search).
/// Prohibited on relational vector backends where EF Core owns schema migrations.
/// </summary>
public interface IVKVectorCollectionLifecycle
{
    /// <summary>
    /// Checks whether the specified collection exists in the dedicated engine.
    /// </summary>
    Task<VKResult<bool>> CollectionExistsAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new named collection with the specified vector dimension and distance metric.
    /// </summary>
    Task<VKResult> CreateCollectionAsync(
        string name,
        int dimensions,
        string distanceMetric = "Cosine",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Drops an existing collection from the dedicated engine.
    /// </summary>
    Task<VKResult> DropCollectionAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates an alias pointing to a physical collection name for blue/green zero-downtime cutover.
    /// </summary>
    Task<VKResult> CreateAliasAsync(
        string aliasName,
        string targetCollectionName,
        CancellationToken cancellationToken = default);
}


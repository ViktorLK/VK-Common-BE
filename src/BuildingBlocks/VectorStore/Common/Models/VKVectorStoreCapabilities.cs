namespace VK.Blocks.VectorStore;

/// <summary>
/// Declares the native execution capabilities of a vector store provider for query pushdown negotiation.
/// </summary>
public sealed record VKVectorStoreCapabilities
{
    /// <summary>
    /// Gets the architectural role and lifecycle ownership pattern of the backend.
    /// </summary>
    public required VKVectorStoreBackendRole BackendRole { get; init; }

    /// <summary>
    /// Gets a value indicating whether the engine natively supports combining vector and keyword searches in a single query.
    /// </summary>
    public bool SupportsNativeHybridSearch { get; init; }

    /// <summary>
    /// Gets a value indicating whether the engine computes Reciprocal Rank Fusion natively on the server side.
    /// </summary>
    public bool SupportsNativeRrf { get; init; }

    /// <summary>
    /// Gets a value indicating whether the engine supports SSD/disk-backed vector indexes (e.g., DiskANN).
    /// </summary>
    public bool SupportsDiskIndex { get; init; }

    /// <summary>
    /// Gets a value indicating whether complex metadata and tenant filters are pushed down into the vector search engine.
    /// </summary>
    public bool SupportsFilterPushdown { get; init; } = true;

    /// <summary>
    /// Standard capability preset for embedded in-memory stores.
    /// </summary>
    public static VKVectorStoreCapabilities EmbeddedDefaults => new()
    {
        BackendRole = VKVectorStoreBackendRole.Embedded,
        SupportsNativeHybridSearch = false,
        SupportsNativeRrf = false,
        SupportsDiskIndex = false,
        SupportsFilterPushdown = false
    };

    /// <summary>
    /// Standard capability preset for dedicated engines like Qdrant.
    /// </summary>
    public static VKVectorStoreCapabilities QdrantDefaults => new()
    {
        BackendRole = VKVectorStoreBackendRole.DedicatedEngine,
        SupportsNativeHybridSearch = true,
        SupportsNativeRrf = true,
        SupportsDiskIndex = false,
        SupportsFilterPushdown = true
    };

    /// <summary>
    /// Standard capability preset for relational stores like PostgreSQL pgvector.
    /// </summary>
    public static VKVectorStoreCapabilities PgvectorDefaults => new()
    {
        BackendRole = VKVectorStoreBackendRole.RelationalColumn,
        SupportsNativeHybridSearch = true,
        SupportsNativeRrf = false,
        SupportsDiskIndex = false,
        SupportsFilterPushdown = true
    };
}

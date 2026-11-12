namespace VK.Blocks.VectorStore;

/// <summary>
/// Defines the architectural role and lifecycle ownership of a vector backend.
/// </summary>
public enum VKVectorStoreBackendRole : byte
{
    /// <summary>
    /// Dedicated vector engine (e.g., Qdrant, Azure AI Search, Cosmos).
    /// VectorStore owns collection creation, aliases, and backup lifecycle.
    /// </summary>
    DedicatedEngine = 0,

    /// <summary>
    /// Relational database vector column (e.g., PostgreSQL pgvector, Azure SQL DiskANN).
    /// Persistence.EFCore owns schema and migrations; VectorStore performs startup validation only.
    /// </summary>
    RelationalColumn = 1,

    /// <summary>
    /// Embedded or in-memory store (e.g., SQLite, InMemory, HnswLite) for dev, test, or edge scenarios.
    /// </summary>
    Embedded = 2
}


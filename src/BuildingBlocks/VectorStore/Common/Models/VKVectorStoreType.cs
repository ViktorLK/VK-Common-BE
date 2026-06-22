namespace VK.Blocks.VectorStore;

/// <summary>
/// Supported vector store types.
/// </summary>
public enum VKVectorStoreType
{
    InMemory,
    Sqlite,
    Qdrant,
    Milvus,
    CosmosDB
}

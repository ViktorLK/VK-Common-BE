namespace VK.Blocks.VectorStore;

/// <summary>
/// Supported vector store types.
/// </summary>
public enum VKVectorStoreType : byte
{
    InMemory = 0,
    Sqlite = 1,
    Qdrant = 2,
    Milvus = 3,
    CosmosDB = 4,
    Pgvector = 5,
    SqlServer = 6,
    AzureAISearch = 7
}

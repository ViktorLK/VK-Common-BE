namespace VK.Blocks.AI.VectorStore.VectorStore.Models;

/// <summary>
/// Supported vector store types.
/// </summary>
public enum VKAIVectorStoreType
{
    InMemory,
    Sqlite,
    Qdrant,
    Milvus,
    CosmosDB
}

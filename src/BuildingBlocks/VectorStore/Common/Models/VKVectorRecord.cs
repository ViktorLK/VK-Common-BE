namespace VK.Blocks.VectorStore;

/// <summary>
/// Represents a record retrieved from a vector store.
/// </summary>
/// <typeparam name="T">The type of the document.</typeparam>
public sealed record VKVectorRecord<T>(
    string Id,
    T Document,
    float Score
);

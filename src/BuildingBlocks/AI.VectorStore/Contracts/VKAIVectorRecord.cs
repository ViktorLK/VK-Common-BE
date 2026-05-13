namespace VK.Blocks.AI.VectorStore;

/// <summary>
/// Represents a raw record in the vector database.
/// </summary>
public sealed record VKAIVectorRecord
{
    public required string Id { get; init; }
    public required string Content { get; init; }
    public required VKAIVectorMetadata Metadata { get; init; }
    public float? Score { get; init; }
}

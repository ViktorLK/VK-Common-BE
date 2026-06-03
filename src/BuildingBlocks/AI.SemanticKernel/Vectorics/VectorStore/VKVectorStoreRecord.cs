using System;
using Microsoft.Extensions.VectorData;

namespace VK.Blocks.AI.SemanticKernel.Vectorics.VectorStore;

/// <summary>
/// Standard VK vector store record schema for semantic search.
/// </summary>
public sealed class VKVectorStoreRecord
{
    [VectorStoreKey]
    public required string Id { get; init; }

    [VectorStoreData(IsIndexed = true)]
    public string? CollectionName { get; init; }

    [VectorStoreData(IsFullTextIndexed = true)]
    public required string Text { get; init; }

    [VectorStoreData]
    public string? Description { get; init; }

    [VectorStoreData]
    public string? AdditionalMetadata { get; init; }

    [VectorStoreVector(1536)]
    public ReadOnlyMemory<float> Embedding { get; init; }
}

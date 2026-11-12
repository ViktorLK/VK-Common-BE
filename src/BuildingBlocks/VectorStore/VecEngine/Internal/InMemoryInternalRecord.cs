using System;
using Microsoft.Extensions.VectorData;

namespace VK.Blocks.VectorStore.VecEngine.Internal;

/// <summary>
/// Record format stored inside the official Microsoft InMemoryVectorStore.
/// </summary>
internal sealed class InMemoryInternalRecord
{
    [VectorStoreKey]
    public string Id { get; set; } = string.Empty;

    [VectorStoreData]
    public string DataJson { get; set; } = string.Empty;

    [VectorStoreData]
    public string TenantId { get; set; } = string.Empty;

    [VectorStoreVector(1536)]
    public ReadOnlyMemory<float> Vector { get; set; } = ReadOnlyMemory<float>.Empty;
}


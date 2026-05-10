using System;

namespace VK.Blocks.AI;

/// <summary>
/// Represents an embedding vector.
/// </summary>
public sealed record VKEmbeddingVector
{
    /// <summary>
    /// Gets the vector values.
    /// </summary>
    public required ReadOnlyMemory<float> Values { get; init; }

    /// <summary>
    /// Gets the dimensionality of the vector.
    /// </summary>
    public int Dimensions => Values.Length;
}

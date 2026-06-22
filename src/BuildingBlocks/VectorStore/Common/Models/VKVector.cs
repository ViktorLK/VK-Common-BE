using System;

namespace VK.Blocks.VectorStore;

/// <summary>
/// Represents a high-dimensional vector.
/// </summary>
public sealed record VKVector
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

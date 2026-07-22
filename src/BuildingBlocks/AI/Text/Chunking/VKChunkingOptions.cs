using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Configuration settings for the Text Chunking feature.
/// </summary>
public sealed partial record VKChunkingOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Chunking is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <inheritdoc />
    public int ChunkSize { get; init; } = 512;

    /// <inheritdoc />
    public int ChunkOverlap { get; init; } = 64;
}

using VK.Blocks.AI.Text.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Configuration settings for the Text Chunking feature.
/// </summary>
[VKFeature(typeof(TextFeature))]
public sealed partial record VKChunkingOptions : IVKChunkingOptions
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

using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest; // [AP.03] flat root namespace for options marker

/// <summary>
/// Options for the AI Ingest Chunking feature.
/// Follows BB.05 (Options pattern with sealed record).
/// </summary>
[VKFeature(typeof(VKVectorIngestBlock), GenerateArgs = true)]
public sealed partial record VKChunkingOptions : IVKChunkingOptions // [BB.07] Options isolation, [AP.01] sealed partial record
{
    /// <summary>
    /// Gets or sets the maximum character size of each chunk.
    /// Default is 500.
    /// </summary>
    public int ChunkSize { get; init; } = 500;

    /// <summary>
    /// Gets or sets the overlap size between consecutive chunks.
    /// Default is 50.
    /// </summary>
    public int ChunkOverlap { get; init; } = 50;
}

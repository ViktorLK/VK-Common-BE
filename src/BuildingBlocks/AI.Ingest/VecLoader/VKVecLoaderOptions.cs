using VK.Blocks.Core;

namespace VK.Blocks.AI.Ingest;

/// <summary>
/// Options for the AI Ingest VecLoader feature.
/// </summary>
[VKFeature(typeof(VKAIIngestBlock))]
public sealed partial record VKVecLoaderOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the chunk size.
    /// </summary>
    public int ChunkSize { get; init; } = 500;

    /// <summary>
    /// Gets the chunk overlap.
    /// </summary>
    public int ChunkOverlap { get; init; } = 50;

    /// <summary>
    /// Gets the maximum document size in bytes allowed for ingestion.
    /// </summary>
    public long MaxDocumentSizeInBytes { get; init; } = 10 * 1024 * 1024; // 10MB

    /// <summary>
    /// Gets the list of allowed document extensions.
    /// </summary>
    public string[] AllowedExtensions { get; init; } = [".pdf", ".txt", ".docx", ".md"];
}

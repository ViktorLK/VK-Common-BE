using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest;

/// <summary>
/// Options for the AI Ingest DocumentLoader feature.
/// </summary>
[VKFeature(typeof(VKVectorIngestBlock))]
public sealed partial record VKDocumentLoaderOptions : IVKBlockOptions
{
    public int ChunkSize { get; init; } = 500;
    public int ChunkOverlap { get; init; } = 50;
    public VKChunkerType ChunkerType { get; init; } = VKChunkerType.Recursive;
    public long MaxDocumentSizeInBytes { get; init; } = 10 * 1024 * 1024; // 10MB
    public string[] AllowedExtensions { get; init; } = [".pdf", ".txt", ".docx", ".md"];
}

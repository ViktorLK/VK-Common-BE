namespace VK.Blocks.AI.Ingest;

/// <summary>
/// Represents the ingestion response.
/// </summary>
public sealed class VKIngestResponse // [AP.01] sealed default
{
    /// <summary>
    /// Gets the count of processed chunks.
    /// </summary>
    public required int ProcessedChunksCount { get; init; } // [AP.01] required keyword
}

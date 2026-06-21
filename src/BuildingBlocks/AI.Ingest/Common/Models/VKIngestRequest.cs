namespace VK.Blocks.AI.Ingest;

/// <summary>
/// Represents the ingestion request.
/// </summary>
public sealed class VKIngestRequest // [AP.01] sealed default
{
    /// <summary>
    /// Gets the document source.
    /// </summary>
    public required string Source { get; init; } // [AP.01] required keyword
}

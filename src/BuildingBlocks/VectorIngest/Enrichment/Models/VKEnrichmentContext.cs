namespace VK.Blocks.VectorIngest; // [AP.03] public API surface in flat root namespace

/// <summary>
/// Context parameters passed during chunk enrichment.
/// </summary>
public sealed record VKEnrichmentContext // [AP.01] sealed record default
{
    /// <summary>
    /// Gets the unique identifier of the source document.
    /// </summary>
    public required string DocumentId { get; init; } // [AP.01] required keyword

    /// <summary>
    /// Gets the total count of chunks extracted from the source document.
    /// </summary>
    public required int TotalChunks { get; init; } // [AP.01] required keyword

    /// <summary>
    /// Gets the source location or URI of the document.
    /// </summary>
    public required string SourceUri { get; init; } // [AP.01] required keyword

    /// <summary>
    /// Gets the document-level content hash.
    /// </summary>
    public required string DocumentHash { get; init; } // [AP.01] required keyword

    /// <summary>
    /// Gets the target collection name.
    /// </summary>
    public string? CollectionName { get; init; }
}

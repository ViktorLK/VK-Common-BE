using System.Collections.Generic;

namespace VK.Blocks.VectorIngest; // [AP.03] public API surface in flat root namespace

/// <summary>
/// Represents the result of loading a document, containing chunks and the document-level hash.
/// </summary>
public sealed record VKLoaderResult // [AP.01] sealed record default
{
    /// <summary>
    /// Gets the list of parsed document chunks.
    /// </summary>
    public required IReadOnlyList<VKChunk> Chunks { get; init; } // [AP.01] required keyword

    /// <summary>
    /// Gets the unique hash of the entire document content.
    /// </summary>
    public required string DocumentHash { get; init; } // [AP.01] required keyword
}

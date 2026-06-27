using System.Collections.Generic;

namespace VK.Blocks.VectorIngest; // [AP.03] public API surface in flat root namespace

/// <summary>
/// Represents a slice of document text with unique identification and relational metadata.
/// </summary>
public sealed record VKChunk // [AP.01] sealed record default
{
    /// <summary>
    /// Gets the unique identifier of the chunk.
    /// </summary>
    public required string Id { get; init; } // [AP.01] required keyword

    /// <summary>
    /// Gets the text content of the chunk.
    /// </summary>
    public required string Content { get; init; } // [AP.01] required keyword

    /// <summary>
    /// Gets the index of the chunk in the document.
    /// </summary>
    public required int ChunkIndex { get; init; } // [AP.01] required keyword

    /// <summary>
    /// Gets the zero-based starting character offset in the source text.
    /// </summary>
    public required int StartOffset { get; init; } // [AP.01] required keyword

    /// <summary>
    /// Gets the zero-based ending character offset (exclusive) in the source text.
    /// </summary>
    public required int EndOffset { get; init; } // [AP.01] required keyword

    /// <summary>
    /// Gets the metadata dictionary containing extra attributes (e.g. parentId, type).
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = [];
}

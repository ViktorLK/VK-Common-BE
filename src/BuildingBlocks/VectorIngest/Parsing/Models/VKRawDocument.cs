using System.Collections.Generic;

namespace VK.Blocks.VectorIngest; // [AP.03] public API surface in flat root namespace

/// <summary>
/// Represents parsed raw document content and source metadata.
/// </summary>
public sealed record VKRawDocument // [AP.01] sealed record default
{
    /// <summary>
    /// Gets the extracted plain text of the document.
    /// </summary>
    public required string PlainText { get; init; } // [AP.01] required keyword

    /// <summary>
    /// Gets the source metadata extracted during parsing.
    /// </summary>
    public required IReadOnlyDictionary<string, object> SourceMetadata { get; init; } // [AP.01] required keyword
}

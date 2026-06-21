using System;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Ingest.Common.Models.Internal;

/// <summary>
/// Pipeline context for the ingestion process.
/// Follows AP.01 and AP.03.
/// </summary>
internal sealed class IngestContext // [AP.01] sealed default
{
    /// <summary>
    /// Gets the document source.
    /// </summary>
    public string Source { get; }

    /// <summary>
    /// Gets or sets the document chunks.
    /// </summary>
    public List<VKVecDocumentChunk> Chunks { get; set; } = [];

    /// <summary>
    /// Gets or sets the generated embeddings.
    /// </summary>
    public IEnumerable<VKEmbeddingsVector> Vectors { get; set; } = Array.Empty<VKEmbeddingsVector>();

    /// <summary>
    /// Initializes a new instance of <see cref="IngestContext"/>.
    /// </summary>
    public IngestContext(string source)
    {
        Source = VKGuard.NotNullOrWhiteSpace(source); // [AP.01] VKGuard boundary
    }
}

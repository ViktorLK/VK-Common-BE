using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest.Common.Models.Internal; // [AP.03] Internal namespace

/// <summary>
/// Pipeline context for the ingestion process.
/// </summary>
internal sealed class IngestContext // [AP.01] sealed default
{
    /// <summary>
    /// Gets the document source.
    /// </summary>
    public string Source { get; }

    /// <summary>
    /// Gets the unique identifier for the document being ingested.
    /// </summary>
    public string DocumentId { get; }

    /// <summary>
    /// Gets or sets the document-level content hash.
    /// </summary>
    public string? DocumentHash { get; set; }

    /// <summary>
    /// Gets or sets the target Vector Store collection name.
    /// </summary>
    public string? CollectionName { get; set; }

    /// <summary>
    /// Gets or sets additional custom metadata to attach to the ingested chunks.
    /// </summary>
    public IReadOnlyDictionary<string, object> CustomMetadata { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// Gets or sets the document chunks.
    /// </summary>
    public List<VKChunk> Chunks { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of <see cref="IngestContext"/>.
    /// </summary>
    public IngestContext(string source, string documentId)
    {
        Source = VKGuard.NotNullOrWhiteSpace(source); // [AP.01] VKGuard boundary
        DocumentId = VKGuard.NotNullOrWhiteSpace(documentId);
    }
}

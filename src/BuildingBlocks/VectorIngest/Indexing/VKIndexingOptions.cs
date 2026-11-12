using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest; // [AP.03] flat root namespace for options marker

/// <summary>
/// Options for the AI Ingest Indexing feature.
/// </summary>
[VKOptions]
public sealed partial record VKIndexingOptions : IVKBlockOptions // [BB.07] Options isolation, [AP.01] sealed partial record
{
    /// <summary>
    /// Gets a value indicating whether sparse vector generation (e.g., BM25/SPLADE) is enabled during indexing.
    /// Default is false to avoid unneeded computational overhead.
    /// </summary>
    public bool EnableSparseVector { get; init; }

    /// <summary>
    /// Gets a value indicating whether token-level multi-vector generation (e.g., ColBERT) is enabled during indexing.
    /// Default is false to prevent token-multiplied storage explosion [BASE-04, BASE-06].
    /// </summary>
    public bool EnableMultiVector { get; init; }
}

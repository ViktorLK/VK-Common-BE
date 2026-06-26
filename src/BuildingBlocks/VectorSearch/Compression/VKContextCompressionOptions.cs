using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Options for the Context Compression stage.
/// </summary>
[VKFeature(typeof(VKVectorSearchBlock), GenerateArgs = true)]
public sealed partial record VKContextCompressionOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the Context Compression stage is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the cosine similarity threshold (0.0 to 1.0) above which a sentence is kept.
    /// </summary>
    public float SimilarityThreshold { get; init; } = 0.6f;

    /// <summary>
    /// Gets or sets the minimum number of sentences to preserve per chunk as a fallback.
    /// </summary>
    public int MinSentences { get; init; } = 1;
}

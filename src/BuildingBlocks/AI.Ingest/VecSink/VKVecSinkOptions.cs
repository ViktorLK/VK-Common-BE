using VK.Blocks.Core;

namespace VK.Blocks.AI.Ingest;

/// <summary>
/// Options for the AI Ingest VecSink feature.
/// </summary>
[VKFeature(typeof(VKAIIngestBlock))]
public sealed partial record VKVecSinkOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the batch size for writing documents/chunks to the vector store.
    /// </summary>
    public int BatchSize { get; init; } = 100;

    /// <summary>
    /// Gets the maximum concurrency level for writing documents/chunks.
    /// </summary>
    public int MaxConcurrency { get; init; } = 4;
}

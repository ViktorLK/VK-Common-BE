using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Options for the Ingesting feature of AI.Corpus.
/// </summary>

public sealed partial record VKIngestingOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the maximum allowed raw content length in characters for ingestion validation.
    /// </summary>
    public int MaxContentLength { get; init; } = 100_000;

    /// <summary>
    /// Gets a value indicating whether to enable the corpus poisoning shield validation.
    /// </summary>
    public bool EnablePoisoningShield { get; init; } = true;
}

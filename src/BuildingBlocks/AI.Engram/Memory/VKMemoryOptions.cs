using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Configuration settings for the Memory feature (Persistence & Search).
/// </summary>
public sealed partial record VKMemoryOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Memory feature is enabled.
    /// Defaults to true.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the default relevance threshold for memory search.
    /// </summary>
    [VKRequestOverride]
    public float? DefaultMinScore { get; init; } = 0.7f;

    /// <summary>
    /// Gets or sets the default maximum number of memories to return in search when TopK is unspecified.
    /// </summary>
    [VKRequestOverride]
    public int? DefaultTopK { get; init; } = 5;

    /// <summary>
    /// Gets or sets the maximum number of long-term memory entries to inject into Psyche prompt context.
    /// Defaults to 5.
    /// </summary>
    [VKRequestOverride]
    public int? MaxMemoryEntriesToInject { get; init; } = 5;

    /// <summary>
    /// Gets or sets a value indicating whether Tiered Gating prefetching is enabled.
    /// Defaults to true.
    /// </summary>
    public bool EnableTieredGating { get; init; } = true;

    /// <summary>
    /// Gets or sets the character count threshold for considering an input a "short input" requiring intent extraction.
    /// Defaults to 15.
    /// </summary>
    public int GatingShortLengthThreshold { get; init; } = 15;

    /// <summary>
    /// Gets or sets the list of pronoun/continuation trigger words that trip the gating threshold for Cue extraction.
    /// </summary>
    public IReadOnlyList<string> GatingKeywords { get; init; } = ["那", "这个", "那个", "它", "还记得", "继续", "然后再", "刚才", "之前"];

    /// <summary>
    /// Gets or sets the speculative timeout in milliseconds for LLM intent Cue extraction before falling back strictly to raw input.
    /// Defaults to 150ms.
    /// </summary>
    public int IntentExtractionTimeoutMs { get; init; } = 150;
}

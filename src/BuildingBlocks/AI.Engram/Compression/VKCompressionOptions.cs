using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Options for the Compression stage.
/// </summary>
public sealed partial record VKCompressionOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the Compression stage is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the compression strategy type.
    /// </summary>
    [VKRequestOverride]
    public VKCompressionStrategyType StrategyType { get; init; } = VKCompressionStrategyType.LlmSummary;

    /// <summary>
    /// Gets or sets the token budget limit before compression is triggered.
    /// </summary>
    public int TokenBudget { get; init; } = 4096;

    /// <summary>
    /// Gets or sets the minimum number of turns before compression is evaluated.
    /// </summary>
    public int MaxTurnsFloor { get; init; } = 10;

    /// <summary>
    /// Gets or sets the number of most recent turns to retain without compression.
    /// </summary>
    public int RetainRecentTurns { get; init; } = 3;

    /// <summary>
    /// Gets or sets the target summary token count threshold for trigger.
    /// </summary>
    public int SummaryTriggerTokenThreshold { get; init; } = 2048;

    /// <summary>
    /// Gets or sets the target tokens for summary generation.
    /// </summary>
    public int SummaryTargetTokens { get; init; } = 512;

    /// <summary>
    /// Gets or sets the maximum input tokens for a single compression job.
    /// </summary>
    public int MaxInputTokensPerJob { get; init; } = 8192;

    /// <summary>
    /// Gets or sets the optional model ID override for chat engine summarization calls.
    /// </summary>
    public string? ModelId { get; init; }

    /// <summary>
    /// Gets or sets the enrichment options for compression.
    /// </summary>
    public VKCompressionEnrichmentOptions Enrichment { get; init; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether automatic background compression is enabled.
    /// </summary>
    public bool EnableAutomaticCompression { get; init; } = true;

    /// <summary>
    /// Gets or sets the interval in minutes for automatic background compression sweeps.
    /// </summary>
    public int AutomaticCompressionIntervalMinutes { get; init; } = 5;
}

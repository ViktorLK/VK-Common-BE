using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Options for the Compression stage.
/// </summary>
[VKFeature(typeof(VKAIEngramBlock))]
public sealed partial record VKCompressionOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the Compression stage is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the strategy type to use for compression.
    /// Defaults to LlmSummary.
    /// </summary>
    public VKCompressionStrategyType StrategyType { get; init; } = VKCompressionStrategyType.LlmSummary;

    /// <summary>
    /// Gets or sets the model identifier specifically used for compression.
    /// If null, the default chat model will be used.
    /// </summary>
    public string? ModelId { get; init; }

    /// <summary>
    /// Gets or sets the token budget allowed for L1 Echo traces before triggering compression.
    /// Defaults to 4000.
    /// </summary>
    public int TokenBudget { get; init; } = 4000;

    /// <summary>
    /// Gets or sets the turn floor before triggering compression.
    /// Defaults to 50.
    /// </summary>
    public int MaxTurnsFloor { get; init; } = 50;

    /// <summary>
    /// Gets or sets the number of recent turns to protect/retain from compression.
    /// Defaults to 8.
    /// </summary>
    public int RetainRecentTurns { get; init; } = 8;

    /// <summary>
    /// Gets or sets the maximum input token count allowed per LLM compression job.
    /// Defaults to 6000.
    /// </summary>
    public int MaxInputTokensPerJob { get; init; } = 6000;

    /// <summary>
    /// Gets or sets a value indicating whether to enable automatic background compression worker.
    /// Defaults to true.
    /// </summary>
    public bool EnableAutomaticCompression { get; init; } = true;

    /// <summary>
    /// Gets or sets the automatic background compression interval in minutes.
    /// Defaults to 30.
    /// </summary>
    public int AutomaticCompressionIntervalMinutes { get; init; } = 30;

    /// <summary>
    /// Gets or sets a value indicating whether to enable timeline extraction.
    /// </summary>
    public bool EnableTimelineExtraction { get; init; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to enable contradiction detection.
    /// </summary>
    public bool EnableContradictionDetection { get; init; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to enable action item extraction.
    /// </summary>
    public bool EnableActionItemExtraction { get; init; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to enable confidence annotation.
    /// </summary>
    public bool EnableConfidenceAnnotation { get; init; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to enable predictive cue extraction.
    /// </summary>
    public bool EnablePredictiveCue { get; init; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to enable incremental compression.
    /// </summary>
    public bool Incremental { get; init; } = false;

    /// <summary>
    /// Gets or sets the maximum length of L2 summary in incremental mode.
    /// </summary>
    public int L2MaxLength { get; init; } = 8000;

    /// <summary>
    /// Gets or sets a value indicating whether to enable salience weighting.
    /// </summary>
    public bool EnableSalienceWeighting { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable emotional tagging during compression.
    /// </summary>
    public bool EnableEmotionalTagging { get; init; } = false;
}

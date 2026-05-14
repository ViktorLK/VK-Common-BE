using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Configuration settings for the Memory feature.
/// </summary>
public sealed record VKMemoryOptions : IVKBlockOptions
{
    /// <summary>
    /// The configuration section name for Memory options.
    /// </summary>
    public static string SectionName => VKAICognitiveOptions.SectionName + ":" + VKAICognitiveOptions.MemorySection;

    /// <summary>
    /// Gets or sets a value indicating whether Memory feature is enabled.
    /// Defaults to true.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the default relevance threshold for memory search.
    /// </summary>
    public float DefaultMinScore { get; init; } = 0.7f;

    /// <summary>
    /// Gets or sets the token threshold to trigger context summarization.
    /// </summary>
    public int SummaryTriggerTokenThreshold { get; init; } = 2048;

    /// <summary>
    /// Gets or sets the target token count for memory summaries.
    /// </summary>
    public int SummaryTargetTokens { get; init; } = 512;

    /// <summary>
    /// Gets or sets the retention period for biometric/sensory memory.
    /// Defaults to 10 minutes.
    /// </summary>
    public int BiometricsRetentionMinutes { get; init; } = 10;

    /// <summary>
    /// Gets or sets the half-life of memory importance in days.
    /// Formula: DecayedImportance = Importance * 2^(-AgeDays / HalfLifeDays).
    /// Defaults to 7.0.
    /// </summary>
    public double HalfLifeDays { get; init; } = 7.0;

    /// <summary>
    /// Gets or sets the threshold (0.0 to 1.0) below which a memory becomes a candidate for pruning.
    /// Defaults to 0.2 (20%).
    /// </summary>
    public float PruningThreshold { get; init; } = 0.2f;

    /// <summary>
    /// Gets or sets the maximum number of long-term memory entries to inject into context.
    /// </summary>
    public int MaxMemoryEntriesToInject { get; init; } = 5;

    /// <summary>
    /// Gets or sets the type of memory store to use.
    /// { Volatile | Sqlite | Cosmos }
    /// </summary>
    public string StoreType { get; init; } = "Volatile";

    /// <summary>
    /// Gets or sets the connection string for the memory store.
    /// For Sqlite, this is the path to the .db file (e.g. "pwp-memory.db").
    /// </summary>
    public string? ConnectionString { get; init; }
}

using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Aggregates all static Memory configuration options.
/// </summary>
public interface IVKMemoryOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets the default relevance threshold for memory search.
    /// </summary>
    float? DefaultMinScore { get; }

    /// <summary>
    /// Gets the token threshold to trigger context summarization.
    /// </summary>
    int? SummaryTriggerTokenThreshold { get; }

    /// <summary>
    /// Gets the target token count for memory summaries.
    /// </summary>
    int? SummaryTargetTokens { get; }

    /// <summary>
    /// Gets the retention period for biometric/sensory memory.
    /// </summary>
    int BiometricsRetentionMinutes { get; }

    /// <summary>
    /// Gets the half-life of memory importance in days.
    /// </summary>
    double HalfLifeDays { get; }

    /// <summary>
    /// Gets the threshold below which a memory becomes a candidate for pruning.
    /// </summary>
    float PruningThreshold { get; }

    /// <summary>
    /// Gets the maximum number of long-term memory entries to inject into context.
    /// </summary>
    int? MaxMemoryEntriesToInject { get; }

    /// <summary>
    /// Gets the type of memory store to use.
    /// </summary>
    string StoreType { get; }

    /// <summary>
    /// Gets the connection string for the memory store.
    /// </summary>
    string? ConnectionString { get; }
}

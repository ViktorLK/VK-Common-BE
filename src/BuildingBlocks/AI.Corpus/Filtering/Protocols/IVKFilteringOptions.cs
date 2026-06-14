using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Aggregates all static Filtering configuration options.
/// </summary>
public interface IVKFilteringOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the default cooldown turns for entries.
    /// </summary>
    int DefaultCooldownTurns { get; }

    /// <summary>
    /// Gets the decay factor for the recency bias filter.
    /// </summary>
    double RecencyDecayFactor { get; }

    /// <summary>
    /// Gets the default probability for entries when not specified.
    /// </summary>
    double DefaultProbability { get; }

    /// <summary>
    /// Gets the maximum number of entries allowed to be injected per turn.
    /// </summary>
    int? MaxEntriesPerTurn { get; }

    /// <summary>
    /// Gets the default sticky turns for entries.
    /// </summary>
    int? DefaultStickyTurns { get; }

    /// <summary>
    /// Gets the maximum number of sticky entries allowed simultaneously.
    /// </summary>
    int? MaxStickyEntries { get; }

    // --- Filter Toggles ---
    bool EnableStickinessFilter { get; }
    bool EnableCooldownFilter { get; }
    bool EnableProbabilityFilter { get; }
    bool EnableGroupFilter { get; }
    bool EnableGlobalExclusionFilter { get; }
    bool EnableDependencyFilter { get; }
    bool EnableEntryMaxCountFilter { get; }
    bool EnableScheduleFilter { get; }
    bool EnableTokenBudgetFilter { get; }
    bool EnableRecencyBiasFilter { get; }
    bool EnableConflictResolutionFilter { get; }
    bool EnableEmotionGatedFilter { get; }
    bool EnableRevealFilter { get; }
    bool EnablePersonaFilter { get; }
    bool EnableFreshnessFilter { get; }
    bool EnableUserSegmentFilter { get; }
    bool EnableDelayFilter { get; }
    bool EnableGroupTopNFilter { get; }
}

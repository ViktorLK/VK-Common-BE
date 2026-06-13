using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Options for the Filtering feature of AI.Corpus.
/// </summary>
[VKFeature(typeof(VKCorpusBlock), GenerateArgs = true, GenerateValidator = true)]
public sealed partial record VKFilteringOptions : IVKFilteringOptions
{
    /// <summary>
    /// Gets the default cooldown turns for entries.
    /// </summary>
    public int DefaultCooldownTurns { get; init; } = 3;

    /// <summary>
    /// Gets the decay factor for the recency bias filter.
    /// </summary>
    public double RecencyDecayFactor { get; init; } = 0.1;

    /// <summary>
    /// Gets the default probability for entries when not specified.
    /// </summary>
    public double DefaultProbability { get; init; } = 1.0;

    /// <summary>
    /// Gets the maximum number of entries allowed to be injected per turn.
    /// </summary>
    public int? MaxEntriesPerTurn { get; init; }

    /// <summary>
    /// Gets the default sticky turns for entries.
    /// </summary>
    public int? DefaultStickyTurns { get; init; }

    /// <summary>
    /// Gets the maximum number of sticky entries allowed simultaneously.
    /// </summary>
    public int? MaxStickyEntries { get; init; }

    // --- Filter Toggles ---
    public bool EnableStickinessFilter { get; init; } = true;
    public bool EnableCooldownFilter { get; init; } = true;
    public bool EnableProbabilityFilter { get; init; } = true;
    public bool EnableGroupFilter { get; init; } = true;
    public bool EnableExclusionFilter { get; init; } = true;
    public bool EnableDependencyFilter { get; init; } = true;
    public bool EnableMaxCountFilter { get; init; } = true;
    public bool EnableScheduleFilter { get; init; } = true;
    public bool EnableTokenBudgetFilter { get; init; } = true;
    public bool EnableRecencyBiasFilter { get; init; } = true;
    public bool EnableConflictResolutionFilter { get; init; } = true;
    public bool EnableEmotionGatedFilter { get; init; } = true;
    public bool EnableRevealFilter { get; init; } = true;
    public bool EnablePersonaFilter { get; init; } = true;
    public bool EnableFreshnessFilter { get; init; } = true;
    public bool EnableUserSegmentFilter { get; init; } = true;
    public bool EnableDelayFilter { get; init; } = true;
    public bool EnableExclusiveGroupFilter { get; init; } = true;
}

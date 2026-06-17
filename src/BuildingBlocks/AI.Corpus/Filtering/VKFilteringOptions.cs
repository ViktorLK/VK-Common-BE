using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Options for the Filtering feature of AI.Corpus.
/// </summary>
[VKFeature(typeof(VKAICorpusBlock), GenerateArgs = true, GenerateValidator = true)]
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

    /// <summary>
    /// Gets a value indicating whether to enable the stickiness filter.
    /// </summary>
    public bool EnableStickinessFilter { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether to enable the cooldown filter.
    /// </summary>
    public bool EnableCooldownFilter { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether to enable the probability filter.
    /// </summary>
    public bool EnableProbabilityFilter { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether to enable the group filter.
    /// </summary>
    public bool EnableGroupFilter { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether to enable the global exclusion filter.
    /// </summary>
    public bool EnableGlobalExclusionFilter { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether to enable the dependency filter.
    /// </summary>
    public bool EnableDependencyFilter { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether to enable the entry max count filter.
    /// </summary>
    public bool EnableEntryMaxCountFilter { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether to enable the schedule filter.
    /// </summary>
    public bool EnableScheduleFilter { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether to enable the token budget filter.
    /// </summary>
    public bool EnableTokenBudgetFilter { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether to enable the recency bias filter.
    /// </summary>
    public bool EnableRecencyBiasFilter { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether to enable the conflict resolution filter.
    /// </summary>
    public bool EnableConflictResolutionFilter { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether to enable the emotion gated filter.
    /// </summary>
    public bool EnableEmotionGatedFilter { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether to enable the reveal filter.
    /// </summary>
    public bool EnableRevealFilter { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether to enable the persona filter.
    /// </summary>
    public bool EnablePersonaFilter { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether to enable the freshness filter.
    /// </summary>
    public bool EnableFreshnessFilter { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether to enable the user segment filter.
    /// </summary>
    public bool EnableUserSegmentFilter { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether to enable the delay filter.
    /// </summary>
    public bool EnableDelayFilter { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether to enable the group top N filter.
    /// </summary>
    public bool EnableGroupTopNFilter { get; init; } = true;
}

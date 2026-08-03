using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Options for memory reclamation (decay, pruning, and background worker).
/// </summary>
public sealed partial record VKReclamationOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether memory reclamation is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the background worker reclamation cycle interval in minutes.
    /// </summary>
    public int ReclamationIntervalMinutes { get; init; } = 15;

    /// <summary>
    /// Gets or sets the maximum batch size per reclamation query iteration.
    /// </summary>
    public int ReclamationBatchSize { get; init; } = 500;

    /// <summary>
    /// Gets or sets the L1 (ShortTerm) half-life decay period in hours.
    /// </summary>
    public double L1HalfLifeHours { get; init; } = 24.0;

    /// <summary>
    /// Gets or sets the L2 (MediumTerm) half-life decay period in hours.
    /// </summary>
    public double L2HalfLifeHours { get; init; } = 168.0; // 7 days

    /// <summary>
    /// Gets or sets the L3 (LongTerm) half-life decay period in hours.
    /// </summary>
    public double L3HalfLifeHours { get; init; } = 720.0; // 30 days

    /// <summary>
    /// Gets or sets the coefficient multiplier applied to log2(1 + AccessCount) for FrequencyBonus calculation.
    /// </summary>
    public double FrequencyBonusCoefficient { get; init; } = 0.05;

    /// <summary>
    /// Gets or sets the pruning score threshold for L1 ShortTerm memories.
    /// </summary>
    public float L1Threshold { get; init; } = 0.1f;

    /// <summary>
    /// Gets or sets the pruning score threshold for L2 MediumTerm memories.
    /// </summary>
    public float L2Threshold { get; init; } = 0.2f;

    /// <summary>
    /// Gets or sets the pruning score threshold for L3 LongTerm memories.
    /// </summary>
    public float L3Threshold { get; init; } = 0.3f;

    /// <summary>
    /// Gets or sets default action for pruned L1 ShortTerm memories.
    /// </summary>
    public VKPruneAction L1Action { get; init; } = VKPruneAction.Delete;

    /// <summary>
    /// Gets or sets default action for pruned L2 MediumTerm memories.
    /// </summary>
    public VKPruneAction L2Action { get; init; } = VKPruneAction.Delete;

    /// <summary>
    /// Gets or sets default action for pruned L3 LongTerm memories.
    /// </summary>
    public VKPruneAction L3Action { get; init; } = VKPruneAction.Archive;

    /// <summary>
    /// Gets or sets the decay formula mode.
    /// </summary>
    public VKDecayMode DecayMode { get; init; } = VKDecayMode.Exponential;

    /// <summary>
    /// Gets or sets persona-specific threshold and half-life overrides.
    /// </summary>
    public IReadOnlyList<VKPersonaReclamationOverride> PersonaOverrides { get; init; } = [];
}

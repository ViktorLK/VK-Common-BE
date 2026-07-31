using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Options for the Scoring stage.
/// </summary>
public sealed partial record VKScoringOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the Scoring stage is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether rule-based scoring and fact routing is enabled.
    /// </summary>
    public bool EnableRuleBasedScoring { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether emotional impact scoring is enabled.
    /// </summary>
    public bool EnableEmotionalScoring { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether LLM heuristic scoring fallback is enabled.
    /// </summary>
    public bool EnableLlmScoring { get; init; } = true;

    /// <summary>
    /// Gets or sets the default score weight.
    /// </summary>
    public double DefaultWeight { get; init; } = 1.0;

    /// <summary>
    /// Gets or sets the default base importance for L1 (ShortTerm) entries when strategy has no opinion.
    /// </summary>
    public double L1DefaultImportance { get; init; } = 0.5;

    /// <summary>
    /// Gets or sets the default base importance for L2 (MediumTerm) entries when strategy has no opinion.
    /// </summary>
    public double L2DefaultImportance { get; init; } = 0.7;

    /// <summary>
    /// Gets or sets the default base importance for L3 (LongTerm) entries when strategy has no opinion.
    /// </summary>
    public double L3DefaultImportance { get; init; } = 0.9;

    /// <summary>
    /// Gets or sets per-persona scoring weight overrides.
    /// </summary>
    public IReadOnlyList<VKPersonaScoringOverride> PersonaOverrides { get; init; } = [];
}

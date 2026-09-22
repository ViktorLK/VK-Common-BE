using System;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Snapshot contract representing the resolved token budget for a Psyche pipeline execution.
/// Acts as the single source of truth for all downstream stages (Echo, Knowledge, Weaving, and external extensions).
/// Follows AP.01 (sealed record default).
/// </summary>
public sealed record VKPsycheTokenBudget
{
    /// <summary>
    /// Gets the resolved total context token limit (physical window or explicit configured budget).
    /// </summary>
    public int? TotalLimit { get; init; }

    /// <summary>
    /// Gets the token budget reserved strictly for model output generation.
    /// </summary>
    public int ReservedResponseTokens { get; init; } = 4096;

    /// <summary>
    /// Gets a value indicating whether a finite total limit is configured.
    /// </summary>
    public bool HasLimit => TotalLimit.HasValue;

    /// <summary>
    /// Gets the available token budget for assembling the prompt (total limit minus reserved response tokens).
    /// Returns <see cref="int.MaxValue"/> if <see cref="TotalLimit"/> is unconstrained.
    /// </summary>
    public int AvailablePromptBudget => TotalLimit.HasValue
        ? Math.Max(0, TotalLimit.Value - ReservedResponseTokens)
        : int.MaxValue;
}

using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Configuration settings for the Token Budgeting feature.
/// </summary>
public sealed partial record VKBudgetingOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Token Budgeting is enabled.
    /// Defaults to false.
    /// </summary>
    public bool Enabled { get; init; } = false;

    /// <summary>
    /// Gets or sets the execution timeout for token budgeting operations.
    /// </summary>
    [VKRequestOverride]
    public TimeSpan? Timeout { get; init; }

    /// <summary>
    /// Gets or sets the default truncation strategy.
    /// </summary>
    [VKRequestOverride]
    public VKTokenBudgetStrategy? DefaultStrategy { get; init; } = VKTokenBudgetStrategy.OldestFirst;

    /// <summary>
    /// Gets or sets the safety margin (in tokens) to subtract from the model's max context.
    /// </summary>
    [VKRequestOverride]
    public int? SafetyMargin { get; init; } = 100;
}

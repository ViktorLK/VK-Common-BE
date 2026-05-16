using System;
using VK.Blocks.AI.Tokenics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Configuration settings for the Token Budgeting feature.
/// </summary>
[VKFeature(typeof(TokenicsFeature), GenerateArgs = true, GenerateValidator = true)]
public sealed partial record VKBudgetingOptions : IVKBudgetingSettings, IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Token Budgeting is enabled.
    /// Defaults to false.
    /// </summary>
    public bool Enabled { get; init; } = false;

    /// <summary>
    /// Gets or sets the execution timeout for token budgeting operations.
    /// </summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>
    /// Gets or sets the default truncation strategy.
    /// </summary>
    public VKTokenBudgetStrategy? DefaultStrategy { get; init; } = VKTokenBudgetStrategy.OldestFirst;

    /// <summary>
    /// Gets or sets the safety margin (in tokens) to subtract from the model's max context.
    /// </summary>
    public int? SafetyMargin { get; init; } = 100;
}

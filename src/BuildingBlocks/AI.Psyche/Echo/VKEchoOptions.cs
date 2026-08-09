using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Options for Echo (Short-term memory conversation history tracking).
/// Follows BB.05 (Options pattern with sealed record).
/// </summary>

public sealed partial record VKEchoOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Echo feature is enabled.
    /// Defaults to true.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the ratio of the total context token limit allocated to short-term memory history.
    /// Mapped as a value between 0.0 and 1.0. Default is 0.3 (30%).
    /// </summary>
    public double TokenBudgetRatio { get; init; } = 0.3;

    /// <summary>
    /// Gets or sets the maximum number of items to retain in the sliding window.
    /// If null, sliding window message-count pruning is disabled, relying entirely on token and turn budgets.
    /// Default is null.
    /// </summary>
    [VKRequestOverride]
    public int? MaxWindowSize { get; init; } = null;

    /// <summary>
    /// Gets or sets an absolute ceiling of tokens allocated for short-term history.
    /// If null, the dynamic budget ratio is used exclusively.
    /// </summary>
    public int? MaxTokens { get; init; } = null;

    /// <summary>
    /// Gets or sets the maximum number of complete conversation turns to retain.
    /// Active only when <see cref="PruneUnit"/> is set to <see cref="VKEchoPruneUnit.Turn"/>.
    /// </summary>
    [VKRequestOverride]
    public int? MaxTurns { get; init; } = null;

    /// <summary>
    /// Gets or sets the unit of truncation/pruning when history exceeds the budget.
    /// Default is <see cref="VKEchoPruneUnit.Turn"/> to maintain dialog consistency.
    /// </summary>
    public VKEchoPruneUnit PruneUnit { get; init; } = VKEchoPruneUnit.Turn;

    /// <summary>
    /// Gets or sets a value indicating whether system messages logged inside dialogue history should be tracked.
    /// Default is false to prevent prompt-bloat.
    /// </summary>
    public bool IncludeSystemMessages { get; init; } = false;

    /// <summary>
    /// Gets or sets the formatting style for rendering dialogue history into prompt text.
    /// Default is <see cref="VKEchoRenderStyle.Header"/>.
    /// </summary>
    public VKEchoRenderStyle RenderStyle { get; init; } = VKEchoRenderStyle.Bracket;

    /// <summary>
    /// Gets or sets a value indicating whether conversation history (user prompt and assistant response) is automatically saved into IVKEchoStore upon pipeline completion.
    /// Default is true.
    /// </summary>
    public bool AutoSaveHistory { get; init; } = true;
}

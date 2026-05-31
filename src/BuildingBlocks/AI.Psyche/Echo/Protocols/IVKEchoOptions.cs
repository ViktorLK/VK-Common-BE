using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Aggregates all static Echo configuration options.
/// </summary>
public interface IVKEchoOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the ratio of the total context token limit allocated to short-term memory history.
    /// </summary>
    double TokenBudgetRatio { get; }

    /// <summary>
    /// Gets the maximum number of items to retain in the sliding window.
    /// </summary>
    int? MaxWindowSize { get; }

    /// <summary>
    /// Gets an absolute ceiling of tokens allocated for short-term history.
    /// </summary>
    int? MaxTokens { get; }

    /// <summary>
    /// Gets the maximum number of complete conversation turns to retain.
    /// </summary>
    int? MaxTurns { get; }

    /// <summary>
    /// Gets the unit of truncation/pruning when history exceeds the budget.
    /// </summary>
    VKEchoPruneUnit PruneUnit { get; }

    /// <summary>
    /// Gets a value indicating whether system messages logged inside dialogue history should be tracked.
    /// </summary>
    bool IncludeSystemMessages { get; }
}

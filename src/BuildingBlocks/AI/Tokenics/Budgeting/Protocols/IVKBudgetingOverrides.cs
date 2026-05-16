namespace VK.Blocks.AI;

/// <summary>
/// Defines budgeting-specific parameters that can be overridden at the request level.
/// </summary>
public interface IVKBudgetingOverrides
{
    /// <summary>
    /// Gets the execution timeout.
    /// </summary>
    System.TimeSpan? Timeout { get; init; }

    /// <summary>
    /// Gets the truncation strategy.
    /// </summary>
    VKTokenBudgetStrategy? DefaultStrategy { get; init; }

    /// <summary>
    /// Gets the safety margin.
    /// </summary>
    int? SafetyMargin { get; init; }
}

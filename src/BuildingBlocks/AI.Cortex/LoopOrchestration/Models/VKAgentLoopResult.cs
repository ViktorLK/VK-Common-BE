using System.Collections.Generic;

namespace VK.Blocks.AI.Cortex;

/// <summary>
/// Final result payload produced from an Agentic loop execution.
/// Follows [AP.01].
/// </summary>
public sealed record VKAgentLoopResult
{
    /// <summary>
    /// Gets the final output message content produced upon loop completion.
    /// </summary>
    public required string FinalContent { get; init; }

    /// <summary>
    /// Gets the total number of iteration steps executed.
    /// </summary>
    public int TotalIterations { get; init; }

    /// <summary>
    /// Gets the chronological list of all iteration steps executed during the loop.
    /// </summary>
    public IReadOnlyList<VKAgentIterationStep> Steps { get; init; } = [];

    /// <summary>
    /// Gets the total aggregated token usage across all iteration steps.
    /// </summary>
    public long TotalTokensUsed { get; init; }

    /// <summary>
    /// Gets a value indicating whether the loop terminated because it hit the maximum iteration limit.
    /// </summary>
    public bool ReachedMaxIterations { get; init; }
}

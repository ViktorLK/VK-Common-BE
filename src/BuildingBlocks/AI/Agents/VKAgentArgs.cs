using System;

namespace VK.Blocks.AI;

/// <summary>
/// Arguments for agent execution.
/// </summary>
public sealed record VKAgentArgs
{
    /// <summary>
    /// Gets the maximum number of iterations.
    /// </summary>
    public int? MaxIterations { get; init; }

    /// <summary>
    /// Gets the maximum number of tool calls per iteration.
    /// </summary>
    public int? MaxToolCallsPerIteration { get; init; }

    /// <summary>
    /// Gets the execution timeout.
    /// </summary>
    public TimeSpan? ExecutionTimeout { get; init; }

    /// <summary>
    /// Gets the maximum total tokens allowed.
    /// </summary>
    public int? MaxTotalTokens { get; init; }

    /// <summary>
    /// Gets whether to allow parallel tool calls.
    /// </summary>
    public bool? AllowParallelToolCalls { get; init; }

    /// <summary>
    /// Gets whether to log detailed tool data.
    /// </summary>
    public bool? LogToolData { get; init; }

    /// <summary>
    /// Gets the tool retry count.
    /// </summary>
    public int? ToolRetryCount { get; init; }
}

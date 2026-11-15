using System;

namespace VK.Blocks.AI;

/// <summary>
/// Per-agent or per-run hard execution guardrails controlling step budgets, token limits,
/// tool-call ceilings, repetition loop detection, tool output truncation, and overall timeout.
/// </summary>
public sealed record VKAgentGuardrailOptions
{
    /// <summary>
    /// Gets the maximum number of model reasoning/response steps allowed in a single run. Defaults to 10.
    /// </summary>
    public int MaxSteps { get; init; } = 10;

    /// <summary>
    /// Gets the maximum cumulative token budget (prompt + completion) permitted for a single run.
    /// </summary>
    public int? MaxTotalTokens { get; init; }

    /// <summary>
    /// Gets the maximum total number of atomic tool invocations allowed in a single run. Defaults to 20.
    /// </summary>
    public int MaxToolCalls { get; init; } = 20;

    /// <summary>
    /// Gets the maximum number of consecutive identical tool invocations (same tool name and arguments)
    /// allowed before aborting with a loop-detection error. Defaults to 3.
    /// </summary>
    public int MaxConsecutiveIdenticalToolCalls { get; init; } = 3;

    /// <summary>
    /// Gets the maximum character length for any individual atomic tool result returned to the model. Defaults to 4000.
    /// </summary>
    public int? MaxToolResultLength { get; init; } = 4000;

    /// <summary>
    /// Gets the explicit execution timeout override for this agent.
    /// </summary>
    public TimeSpan? Timeout { get; init; }
}

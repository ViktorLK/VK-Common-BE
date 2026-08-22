using System;

namespace VK.Blocks.AI.Cortex;

/// <summary>
/// Record representing an individual iteration step executed within an Agentic loop.
/// Follows [AP.01].
/// </summary>
public sealed record VKAgentIterationStep
{
    /// <summary>
    /// Gets the 1-based iteration index.
    /// </summary>
    public required int StepIndex { get; init; }

    /// <summary>
    /// Gets the turn result produced during this iteration step.
    /// </summary>
    public required VKChatTurnResult TurnResult { get; init; }

    /// <summary>
    /// Gets the timestamp when this step finished execution.
    /// </summary>
    public DateTimeOffset ExecutedAt { get; init; } = DateTimeOffset.UtcNow;
}

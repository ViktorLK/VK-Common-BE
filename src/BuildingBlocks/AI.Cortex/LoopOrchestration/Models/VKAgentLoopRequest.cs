using System;

namespace VK.Blocks.AI.Cortex;

/// <summary>
/// Fully-resolved request payload for executing a multi-turn iterative Agentic loop.
/// Follows [AP.01] (sealed record).
/// </summary>
public sealed record VKAgentLoopRequest
{
    /// <summary>
    /// Gets the initial turn request providing session context, prompt parameters, and initial user prompt.
    /// </summary>
    public required VKChatTurnRequest InitialRequest { get; init; }

    /// <summary>
    /// Gets the optional maximum iteration limit for this specific loop execution.
    /// If null, <see cref="VKLoopOrchestrationOptions.DefaultMaxIterations"/> is applied.
    /// </summary>
    public int? MaxIterations { get; init; }
}

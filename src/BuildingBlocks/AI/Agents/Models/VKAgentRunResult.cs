using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Detailed execution outcome produced by <see cref="IVKAgent.RunAsync"/> including message transcript,
/// completed step count, tool invocation count, token usage, and resolved thread identifier.
/// </summary>
public sealed record VKAgentRunResult
{
    /// <summary>
    /// Gets the final text output produced by the agent.
    /// </summary>
    public required string Output { get; init; }

    /// <summary>
    /// Gets the messages produced or observed during the run.
    /// </summary>
    public IReadOnlyList<VKChatMessage> Messages { get; init; } = [];

    /// <summary>
    /// Gets the total number of model steps executed.
    /// </summary>
    public int CompletedSteps { get; init; }

    /// <summary>
    /// Gets the total number of atomic tool invocations executed.
    /// </summary>
    public int ToolCallsCount { get; init; }

    /// <summary>
    /// Gets the cumulative token usage for the run, when reported by the provider.
    /// </summary>
    public VKAITokenUsage? Usage { get; init; }

    /// <summary>
    /// Gets the thread/session identifier associated with the run, if any.
    /// </summary>
    public string? ThreadId { get; init; }

    /// <summary>
    /// Gets the completion finish reason reported by the runtime/model.
    /// </summary>
    public string? FinishReason { get; init; }
}

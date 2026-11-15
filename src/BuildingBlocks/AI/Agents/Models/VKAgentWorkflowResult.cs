using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Result returned by <see cref="IVKAgentWorkflowRunner"/> containing final output, session tracking,
/// last recorded checkpoint identifier, node outputs, and step count.
/// </summary>
public sealed record VKAgentWorkflowResult
{
    /// <summary>
    /// Gets the final text output emitted by the workflow.
    /// </summary>
    public required string Output { get; init; }

    /// <summary>
    /// Gets the workflow run/session identifier used to correlate and resume checkpoints.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// Gets the identifier of the latest recorded workflow checkpoint, if checkpointing is enabled.
    /// </summary>
    public string? LastCheckpointId { get; init; }

    /// <summary>
    /// Gets ordered node messages emitted during workflow execution.
    /// </summary>
    public IReadOnlyList<VKChatMessage> Messages { get; init; } = [];

    /// <summary>
    /// Gets the total number of workflow super-steps/nodes completed.
    /// </summary>
    public int CompletedSteps { get; init; }
}

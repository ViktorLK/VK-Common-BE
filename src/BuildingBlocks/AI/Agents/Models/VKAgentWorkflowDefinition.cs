using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Declarative graph definition for executing deterministic multi-agent workflows with checkpointing support.
/// </summary>
public sealed record VKAgentWorkflowDefinition
{
    /// <summary>
    /// Gets the logical name of the workflow.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the name of the entry agent node where execution begins.
    /// </summary>
    public required string EntryAgentName { get; init; }

    /// <summary>
    /// Gets the collection of participating agent nodes keyed by <see cref="IVKAgent.Name"/>.
    /// </summary>
    public required IReadOnlyList<IVKAgent> Agents { get; init; }

    /// <summary>
    /// Gets the directed transitions connecting workflow agent nodes.
    /// </summary>
    public IReadOnlyList<VKAgentWorkflowEdge> Edges { get; init; } = [];

    /// <summary>
    /// Gets the maximum number of workflow execution steps allowed before termination. Defaults to 20.
    /// </summary>
    public int MaxSteps { get; init; } = 20;

    /// <summary>
    /// Gets a value indicating whether workflow checkpoints should be recorded after each super-step. Defaults to true.
    /// </summary>
    public bool EnableCheckpointing { get; init; } = true;
}

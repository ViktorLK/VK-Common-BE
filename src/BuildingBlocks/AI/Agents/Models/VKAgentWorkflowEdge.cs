using System;

namespace VK.Blocks.AI;

/// <summary>
/// Represents a directed transition edge between two agent nodes in a deterministic workflow graph.
/// </summary>
public sealed record VKAgentWorkflowEdge
{
    /// <summary>
    /// Gets the name of the source agent node.
    /// </summary>
    public required string SourceAgentName { get; init; }

    /// <summary>
    /// Gets the name of the target agent node.
    /// </summary>
    public required string TargetAgentName { get; init; }

    /// <summary>
    /// Gets an optional predicate evaluated against the source agent output to activate this edge.
    /// When omitted, the edge transitions unconditionally.
    /// </summary>
    public Func<string, bool>? Condition { get; init; }

    /// <summary>
    /// Gets an optional human-readable label identifying the transition edge.
    /// </summary>
    public string? Label { get; init; }
}

using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Input metadata passed to <see cref="IVKAgentContextProvider"/> implementations before an agent executes.
/// </summary>
public sealed record VKAgentContextRequest
{
    /// <summary>
    /// Gets the name of the agent about to run.
    /// </summary>
    public required string AgentName { get; init; }

    /// <summary>
    /// Gets the normalized input prompt for the current run.
    /// </summary>
    public required string Input { get; init; }

    /// <summary>
    /// Gets the optional thread or session identifier.
    /// </summary>
    public string? ThreadId { get; init; }

    /// <summary>
    /// Gets the optional tenant identifier propagated from the boundary.
    /// </summary>
    public string? TenantId { get; init; }

    /// <summary>
    /// Gets the optional trace/correlation identifier propagated from the boundary.
    /// </summary>
    public string? TraceId { get; init; }

    /// <summary>
    /// Gets read-only caller variables present on the execution context.
    /// </summary>
    public IReadOnlyDictionary<string, object>? Variables { get; init; }
}

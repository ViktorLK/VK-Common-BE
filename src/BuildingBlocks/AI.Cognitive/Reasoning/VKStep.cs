using System.Collections.Generic;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Represents a single, atomic step or instruction within an AI plan.
/// </summary>
public sealed record VKStep
{
    /// <summary>
    /// Gets the unique identifier for the step.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the name/title of the step.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the description or detailed instruction for this step.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the name of the tool or agent required to execute this step (if any).
    /// </summary>
    public string? Target { get; init; }

    /// <summary>
    /// Gets the input arguments for this step.
    /// </summary>
    public IDictionary<string, object> Parameters { get; init; } = new Dictionary<string, object>();

    /// <summary>
    /// Gets the dependencies of this step (IDs of steps that must be completed first).
    /// </summary>
    public IReadOnlyList<string> DependsOn { get; init; } = [];
}

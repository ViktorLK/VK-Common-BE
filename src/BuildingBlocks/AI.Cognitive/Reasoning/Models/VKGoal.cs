using System.Collections.Generic;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Represents a high-level goal decomposed into a sequence of executable steps.
/// </summary>
public sealed record VKGoal
{
    /// <summary>
    /// Gets the unique identifier for the goal.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the title of the goal.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets the original goal description/instruction.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the collection of steps required to achieve the goal.
    /// </summary>
    public required IReadOnlyList<VKStep> Steps { get; init; }

    /// <summary>
    /// Gets the current status of the goal.
    /// </summary>
    public string Status { get; init; } = "Planned";
}

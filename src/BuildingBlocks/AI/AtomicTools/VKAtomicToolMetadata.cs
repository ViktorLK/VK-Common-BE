using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Defines the structural metadata for an atomic tool.
/// Following the "Industrial DNA" (Typed Metadata Pattern).
/// </summary>
public sealed record VKAtomicToolMetadata
{
    /// <summary>
    /// Gets the name of the tool.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the description of what the tool does.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the category of the tool.
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// Gets the list of tags associated with the tool.
    /// </summary>
    public IReadOnlyList<string> Tags { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether the tool execution requires explicit human confirmation.
    /// </summary>
    public bool RequiresConfirmation { get; init; } = false;

    /// <summary>
    /// Gets the specific timeout for this tool in milliseconds.
    /// </summary>
    public int? TimeoutMilliseconds { get; init; }
}

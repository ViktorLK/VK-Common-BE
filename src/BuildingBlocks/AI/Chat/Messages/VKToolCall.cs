using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Represents a call to a tool/function requested by the AI.
/// </summary>
public sealed record VKToolCall
{
    /// <summary>
    /// Gets the unique identifier for this tool call.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the name of the tool to be called.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the arguments for the tool call.
    /// </summary>
    public required IDictionary<string, object> Arguments { get; init; }
}

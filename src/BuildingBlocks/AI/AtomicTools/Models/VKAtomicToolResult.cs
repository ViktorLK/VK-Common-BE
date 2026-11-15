using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Represents the result of an atomic tool execution.
/// </summary>
public sealed record VKAtomicToolResult
{
    /// <summary>
    /// Gets the content returned by the tool.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Gets the structured payload returned by the tool, aligned with ReturnSchema.
    /// </summary>
    public object? StructuredContent { get; init; }

    /// <summary>
    /// Gets a value indicating whether the result represents a tool-level error exposed to the model.
    /// </summary>
    public bool IsToolError { get; init; }

    /// <summary>
    /// Gets any additional metadata returned by the tool.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();
}

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
    /// Gets any additional metadata returned by the tool.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();
}

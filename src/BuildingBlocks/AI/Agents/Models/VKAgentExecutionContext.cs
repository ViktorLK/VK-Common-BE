using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Context for agent execution, composing the underlying <see cref="VKAtomicToolContext"/>.
/// </summary>
public sealed record VKAgentExecutionContext
{
    /// <summary>
    /// Gets the underlying atomic tool execution context.
    /// </summary>
    public VKAtomicToolContext ToolContext { get; init; } = new();

    /// <summary>
    /// Gets the variables/state available to the agent.
    /// </summary>
    public Dictionary<string, object> Variables => ToolContext.Variables;

    /// <summary>
    /// Gets the history of tool calls and results.
    /// </summary>
    public List<VKAtomicToolResult> ToolCallHistory => ToolContext.ToolCallHistory;
}

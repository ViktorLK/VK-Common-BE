using System.Collections.Generic;

namespace VK.Blocks.AI;


/// <summary>
/// Context for agent execution.
/// </summary>
public sealed record VKAgentExecutionContext
{
    /// <summary>
    /// Gets the variables/state available to the agent.
    /// </summary>
    public Dictionary<string, object> Variables { get; init; } = [];

    /// <summary>
    /// Gets the history of tool calls and results.
    /// </summary>
    public List<VKAgentToolResult> ToolCallHistory { get; init; } = [];
}

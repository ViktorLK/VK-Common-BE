namespace VK.Blocks.AI;

/// <summary>
/// Defines the selection modes for multi-agent execution.
/// </summary>
public enum VKAgentSelectionMode
{
    /// <summary>
    /// Cycle through agents in a round-robin fashion.
    /// </summary>
    RoundRobin,

    /// <summary>
    /// Use an LLM to dynamically determine the next agent to execute.
    /// </summary>
    LLMBased
}

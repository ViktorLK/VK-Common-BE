namespace VK.Blocks.AI;

/// <summary>
/// Defines the selection modes for multi-agent execution.
/// </summary>
public enum VKAgentSelectionMode : byte
{
    /// <summary>
    /// Cycle through agents in a round-robin fashion.
    /// </summary>
    RoundRobin = 0,

    /// <summary>
    /// Use an LLM to dynamically determine the next agent to execute.
    /// </summary>
    LLMBased = 1
}

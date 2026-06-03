using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Standard error constants for the Agents feature.
/// </summary>
public static class VKAgentErrors
{
    /// <summary>
    /// Error returned when the agent execution fails.
    /// </summary>
    public static readonly VKError ExecutionFailed = new("AI.Agents.ExecutionFailed", "The agent execution failed.");

    /// <summary>
    /// Error returned when the maximum number of iterations is reached.
    /// </summary>
    public static readonly VKError MaxIterationsReached = new("AI.Agents.MaxIterationsReached", "The agent reached the maximum number of iterations.");

    /// <summary>
    /// Error returned when the agent task times out.
    /// </summary>
    public static readonly VKError Timeout = new("AI.Agents.Timeout", "The agent execution timed out.", VKErrorType.Timeout);

    /// <summary>
    /// Error returned when the agent group chat execution fails.
    /// </summary>
    public static readonly VKError GroupChatFailed = new("AI.Agents.GroupChatFailed", "The cooperative agent group execution failed.");
}

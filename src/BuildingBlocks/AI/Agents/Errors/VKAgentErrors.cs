using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Standard error constants for the Agents feature and agent runtimes.
/// Adheres to CS.01 and FS-02: centralized immutable constants.
/// </summary>
public static class VKAgentErrors
{
    /// <summary>
    /// Error returned when the agent execution fails.
    /// </summary>
    public static readonly VKError ExecutionFailed = new("AI.Agents.ExecutionFailed", "The agent execution failed.");

    /// <summary>
    /// Error returned when the maximum number of iterations/steps is reached.
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

    /// <summary>
    /// Error returned when an agent with the requested name is not found.
    /// </summary>
    public static readonly VKError AgentNotFound = VKError.NotFound("AI.Agents.NotFound", "The requested agent definition was not found.");

    /// <summary>
    /// Error returned when multi-agent workflow orchestration fails.
    /// </summary>
    public static readonly VKError WorkflowFailed = VKError.Failure("AI.Agents.WorkflowFailed", "Multi-agent workflow orchestration failed.");

    /// <summary>
    /// Error returned when an agent execution exceeds its total token budget.
    /// </summary>
    public static readonly VKError TokenBudgetExceeded = VKError.Failure("AI.Agents.TokenBudgetExceeded", "The agent exceeded its configured total token budget.");

    /// <summary>
    /// Error returned when an agent reaches the maximum permitted tool call count in a single run.
    /// </summary>
    public static readonly VKError MaxToolCallsReached = VKError.Failure("AI.Agents.MaxToolCallsReached", "The agent reached the maximum number of tool calls allowed for a single run.");

    /// <summary>
    /// Error returned when an agent is aborted due to repetitive identical tool invocation loop detection.
    /// </summary>
    public static readonly VKError LoopDetected = VKError.Failure("AI.Agents.LoopDetected", "The agent was aborted due to repetitive identical tool invocation loop detection.");

    /// <summary>
    /// Error returned when tool execution is denied or lacks required confirmation.
    /// </summary>
    public static readonly VKError ToolApprovalDenied = VKError.Forbidden("AI.Agents.ToolApprovalDenied", "Tool execution was denied or lacked required approval.");

    /// <summary>
    /// Error returned when dynamic next-agent selection fails without fallback permission.
    /// </summary>
    public static readonly VKError AgentSelectionFailed = VKError.Failure("AI.Agents.SelectionFailed", "Failed to determine the next agent to execute.");

    /// <summary>
    /// Error returned when a requested workflow checkpoint or session state is not found.
    /// </summary>
    public static readonly VKError CheckpointNotFound = VKError.NotFound("AI.Agents.CheckpointNotFound", "The requested workflow checkpoint or session state was not found.");

    /// <summary>
    /// Error returned when no IChatClient is registered for agent execution.
    /// </summary>
    public static readonly VKError ChatClientNotRegistered = VKError.Failure("AI.Agents.ChatClientNotRegistered", "No IChatClient is registered for agent execution.");

    /// <summary>
    /// Creates an error returned when no keyed IChatClient is registered for the requested model profile.
    /// </summary>
    public static VKError ProfileNotFound(string profile) =>
        VKError.NotFound("AI.Agents.ProfileNotFound", $"No keyed IChatClient is registered for model profile '{profile}'.");

    /// <summary>
    /// Creates an execution failure error with a custom detail message.
    /// </summary>
    public static VKError ExecutionFailure(string detail) =>
        VKError.Failure("AI.Agents.ExecutionFailed", detail);

    /// <summary>
    /// Creates a workflow failure error with a custom detail message.
    /// </summary>
    public static VKError WorkflowFailure(string detail) =>
        VKError.Failure("AI.Agents.WorkflowFailed", detail);

    /// <summary>
    /// Creates a validation error for invalid agent arguments or configuration.
    /// </summary>
    public static VKError InvalidArgument(string message) =>
        VKError.Validation("AI.Agents.InvalidArgument", message);
}

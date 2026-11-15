namespace VK.Blocks.AI;

/// <summary>
/// Represents an observable event yielded during streaming agent execution.
/// </summary>
public sealed record VKAgentStreamEvent
{
    /// <summary>
    /// Gets the event classification.
    /// </summary>
    public required VKAgentStreamEventKind Kind { get; init; }

    /// <summary>
    /// Gets the name of the agent emitting the event.
    /// </summary>
    public string? AgentName { get; init; }

    /// <summary>
    /// Gets the 1-based step index associated with the event, when applicable.
    /// </summary>
    public int StepIndex { get; init; }

    /// <summary>
    /// Gets the incremental or final text payload associated with the event.
    /// </summary>
    public string? TextDelta { get; init; }

    /// <summary>
    /// Gets the atomic tool name for tool execution events.
    /// </summary>
    public string? ToolName { get; init; }

    /// <summary>
    /// Gets the tool invocation identifier for correlating start and completion events.
    /// </summary>
    public string? ToolCallId { get; init; }

    /// <summary>
    /// Gets the formatted output produced by the tool execution event.
    /// </summary>
    public string? ToolOutput { get; init; }

    /// <summary>
    /// Gets token consumption telemetry if reported on completion events.
    /// </summary>
    public VKAITokenUsage? Usage { get; init; }
}

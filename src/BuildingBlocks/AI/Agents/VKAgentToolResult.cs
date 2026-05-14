namespace VK.Blocks.AI;

/// <summary>
/// Represents the result of a tool execution by an agent.
/// </summary>
public sealed record VKAgentToolResult
{
    /// <summary>
    /// Gets the result content.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Gets a value indicating whether the execution was successful.
    /// </summary>
    public bool IsSuccess { get; init; } = true;

    /// <summary>
    /// Gets the error message if the execution failed.
    /// </summary>
    public string? ErrorMessage { get; init; }
}

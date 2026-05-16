namespace VK.Blocks.AI;

/// <summary>
/// Represents the result of a tool execution to be sent back to the AI.
/// </summary>
public sealed record VKToolResult
{
    /// <summary>
    /// Gets the unique identifier of the tool call this result belongs to.
    /// </summary>
    public required string CallId { get; init; }

    /// <summary>
    /// Gets the name of the tool.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the result content (usually a JSON string).
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

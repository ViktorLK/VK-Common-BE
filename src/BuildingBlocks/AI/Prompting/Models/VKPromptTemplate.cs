using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Represents a loaded prompt template along with its metadata and default parameters.
/// </summary>
public sealed record VKPromptTemplate
{
    /// <summary>
    /// Gets the unique identifier for this prompt template.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the version string of the prompt template.
    /// </summary>
    public required string Version { get; init; }

    /// <summary>
    /// Gets the raw template text containing variables to be substituted.
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// Gets the intended role for this prompt (e.g., System, User).
    /// </summary>
    public required VKChatRole Role { get; init; }

    /// <summary>
    /// Gets the default variables and configuration values defined for this template.
    /// </summary>
    public IDictionary<string, object?> DefaultVariables { get; init; } = new Dictionary<string, object?>();
}

namespace VK.Blocks.AI;

/// <summary>
/// Configuration options for creating a Prompt-based Tool.
/// </summary>
public sealed record VKPromptToolOptions
{
    /// <summary>
    /// Gets the unique identifier of the prompt template to use.
    /// </summary>
    public required string PromptId { get; init; }

    /// <summary>
    /// Gets the optional version of the prompt template.
    /// </summary>
    public string? PromptVersion { get; init; }

    /// <summary>
    /// Gets the tool manifest defining the tool's schema, description, and metadata.
    /// </summary>
    public required VKAtomicToolManifest Manifest { get; init; }
}

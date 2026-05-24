namespace VK.Blocks.AI;

/// <summary>
/// Represents the pure definition/schema of an atomic tool.
/// This part is "what the model sees".
/// </summary>
public sealed record VKAtomicToolManifest
{
    /// <summary>
    /// Gets the governance and descriptive metadata for the tool.
    /// </summary>
    public required VKAtomicToolMetadata Metadata { get; init; }

    /// <summary>
    /// Gets the JSON Schema for the tool parameters.
    /// </summary>
    public string? ParameterSchema { get; init; }

    /// <summary>
    /// Gets the JSON Schema for the tool return value.
    /// </summary>
    public string? ReturnSchema { get; init; }
}

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Arguments for persona execution.
/// </summary>
public sealed record VKPersonaArgs
{
    /// <summary>
    /// Gets the persona identifier to use (overrides default).
    /// </summary>
    public string? PersonaId { get; init; }
}

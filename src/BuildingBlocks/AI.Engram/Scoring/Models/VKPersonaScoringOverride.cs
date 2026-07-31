namespace VK.Blocks.AI.Engram;

/// <summary>
/// Per-persona scoring weight override.
/// </summary>
public sealed record VKPersonaScoringOverride
{
    /// <summary>
    /// Gets the persona identifier to match against ExtendedScope["PersonaId"].
    /// </summary>
    public required string PersonaId { get; init; }

    /// <summary>
    /// Gets the multiplier applied to the base importance (> 1.0 = boost importance, < 1.0 = reduce).
    /// </summary>
    public double ImportanceMultiplier { get; init; } = 1.0;
}

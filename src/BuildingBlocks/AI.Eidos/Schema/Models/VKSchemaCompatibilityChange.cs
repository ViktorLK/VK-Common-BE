namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Individual schema difference detected during compatibility evaluation.
/// Complies with AP.01 (sealed record).
/// </summary>
public sealed record VKSchemaCompatibilityChange
{
    /// <summary>
    /// JSONPath or property coordinate where the difference occurred.
    /// </summary>
    public required string PropertyPath { get; init; }

    /// <summary>
    /// Impact severity of this specific change.
    /// </summary>
    public required VKSchemaCompatibilityLevel Level { get; init; }

    /// <summary>
    /// Human-readable explanation of the schema difference.
    /// </summary>
    public required string Description { get; init; }
}

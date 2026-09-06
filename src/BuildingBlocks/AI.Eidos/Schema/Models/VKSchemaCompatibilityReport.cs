using System.Collections.Generic;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Evaluation report analyzing compatibility between two contract schemas.
/// Complies with AP.01 (sealed record).
/// </summary>
public sealed record VKSchemaCompatibilityReport
{
    /// <summary>
    /// Overall compatibility classification.
    /// </summary>
    public required VKSchemaCompatibilityLevel Level { get; init; }

    /// <summary>
    /// True if changes are either Identical or Compatible (no breaking changes).
    /// </summary>
    public bool IsCompatible => Level != VKSchemaCompatibilityLevel.Breaking;

    /// <summary>
    /// All detected differences between the two schemas.
    /// </summary>
    public IReadOnlyList<VKSchemaCompatibilityChange> Changes { get; init; } = [];
}

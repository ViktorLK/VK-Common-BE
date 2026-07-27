using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Persona-level pruning threshold override configuration.
/// </summary>
public sealed record VKPersonaPruningOverride
{
    /// <summary>
    /// Gets the target Persona Identifier.
    /// </summary>
    public required string PersonaId { get; init; }

    /// <summary>
    /// Gets the custom L1 RetentionScore pruning threshold for this persona.
    /// </summary>
    public float? L1Threshold { get; init; }

    /// <summary>
    /// Gets the custom L2 RetentionScore pruning threshold for this persona.
    /// </summary>
    public float? L2Threshold { get; init; }

    /// <summary>
    /// Gets the custom L3 RetentionScore pruning threshold for this persona.
    /// </summary>
    public float? L3Threshold { get; init; }
}

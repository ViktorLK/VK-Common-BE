using System;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Execution arguments passed via VKPsycheRequest.WithArgs to specify Eidos contract governance coordinates.
/// Complies with AP.01 (sealed record).
/// </summary>
public sealed record VKAIEidosRequestArgs
{
    /// <summary>
    /// Gets the business scenario identifier to resolve contract for.
    /// </summary>
    public required string Scenario { get; init; }

    /// <summary>
    /// Gets an optional explicit contract overriding DB/Registry resolution.
    /// </summary>
    public VKAIEidosResponseContract? ExplicitContract { get; init; }

    /// <summary>
    /// Gets an optional target DTO type for strong-typed binding.
    /// </summary>
    public Type? TargetType { get; init; }

    /// <summary>
    /// Gets a value indicating whether to automatically inject NarrativeText field into the projected JSON schema.
    /// </summary>
    public bool InjectNarrativeField { get; init; } = false;
}

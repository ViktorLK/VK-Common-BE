namespace VK.Blocks.AI.Engram;

/// <summary>
/// Persona-level memory reclamation (decay and pruning) override configuration.
/// </summary>
public sealed record VKPersonaReclamationOverride
{
    /// <summary>
    /// Gets the target Persona Identifier.
    /// </summary>
    public required string PersonaId { get; init; }

    /// <summary>
    /// Gets the custom L1 ShortTerm half-life decay period in hours for this persona.
    /// </summary>
    public double? L1HalfLifeHours { get; init; }

    /// <summary>
    /// Gets the custom L2 MediumTerm half-life decay period in hours for this persona.
    /// </summary>
    public double? L2HalfLifeHours { get; init; }

    /// <summary>
    /// Gets the custom L3 LongTerm half-life decay period in hours for this persona.
    /// </summary>
    public double? L3HalfLifeHours { get; init; }

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

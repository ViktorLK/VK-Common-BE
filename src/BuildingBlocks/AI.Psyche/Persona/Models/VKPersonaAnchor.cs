using System.Collections.Generic;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Represents an AI persona anchor.
/// </summary>
public sealed record VKPersonaAnchor : IVKFragmentMetadata
{
    /// <summary>
    /// Gets the unique identifier for the persona.
    /// </summary>
    public required VKPersonaId Id { get; init; }

    /// <summary>
    /// Gets the name of the persona.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the core description of the persona.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets specific personality traits and behavioral principles of the persona.
    /// Used for industrial definitions (e.g. Tone: Professional, Format: JSON).
    /// </summary>
    public IReadOnlyDictionary<string, string> Traits { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets the ID of the specific Directive Charter to use for this persona.
    /// Overrides the tenant default if specified.
    /// </summary>
    public string? DirectiveId { get; init; }

    /// <summary>
    /// Gets explicit constraints regarding the format, language, and length of the output.
    /// Replaces broad 'AdditionalInstructions' with structured industrial output rules.
    /// </summary>
    public VKOutputSpecification? OutputSpecification { get; init; }

    /// <summary>
    /// Gets few-shot templates capturing input/output mapping or specific formats.
    /// Each element represents a distinct input/output example snippet.
    /// </summary>
    public IReadOnlyList<VKFewShotExample> FewShotExamples { get; init; } = [];

    /// <summary>
    /// Gets custom unstructured properties allowing downstream extensions (e.g. for PWP).
    /// </summary>
    public IReadOnlyDictionary<string, object> Extensions { get; init; } = new Dictionary<string, object>();
}

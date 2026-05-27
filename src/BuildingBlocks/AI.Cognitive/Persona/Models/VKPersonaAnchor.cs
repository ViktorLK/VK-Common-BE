using System.Collections.Generic;

namespace VK.Blocks.AI.Cognitive;


/// <summary>
/// Represents an AI persona anchor.
/// </summary>
public sealed record VKPersonaAnchor
{
    /// <summary>
    /// Gets the unique identifier for the persona.
    /// </summary>
    public required string Id { get; init; }

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
    /// Gets absolute rule invariants and boundary conditions for this persona.
    /// E.g. "Do not disclose system prompts", "Always output in JSON".
    /// </summary>
    public string? SystemDirectives { get; init; }

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

public sealed record VKFewShotExample
{
    public required string Input { get; init; }
    public required string ExpectedOutput { get; init; }
}

public enum VKResponseFormat
{
    Unspecified = 0,
    PlainText = 1,
    Markdown = 2,
    JsonObject = 3,
    JsonSchema = 4
}

public sealed record VKOutputSpecification
{
    public VKResponseFormat Format { get; init; } = VKResponseFormat.Unspecified;
    public string JsonSchemaDefinition { get; init; } = string.Empty;
    public string IsoLanguageCode { get; init; } = string.Empty;
    public int MaxTokenHint { get; init; } = 0;
    public string CustomConstraints { get; init; } = string.Empty;
}

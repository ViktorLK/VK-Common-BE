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
    /// Gets the core description/personality of the persona.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets specific personality traits and behavioral principles of the persona.
    /// </summary>
    public string Personality { get; init; } = string.Empty;

    /// <summary>
    /// Gets the very first message sent in a new session to define tone and format.
    /// </summary>
    public string? FirstMessage { get; init; }

    /// <summary>
    /// Gets few-shot dialogue templates capturing unique speech styles.
    /// Each element represents a distinct dialogue example/snippet.
    /// </summary>
    public IReadOnlyList<string> DialogueExamples { get; init; } = [];

    /// <summary>
    /// Gets custom developer instructions/invariants for this specific persona.
    /// </summary>
    public string? SystemPrompt { get; init; }

    /// <summary>
    /// Gets the knowledge entries associated with the persona.
    /// </summary>
    public IEnumerable<VKKnowledgeEntry> Knowledge { get; init; } = [];

    /// <summary>
    /// Gets the evolving traits of the persona.
    /// </summary>
    public IDictionary<string, string> Traits { get; init; } = new Dictionary<string, string>();
}

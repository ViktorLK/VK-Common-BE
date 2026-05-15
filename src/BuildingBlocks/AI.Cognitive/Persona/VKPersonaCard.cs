using System.Collections.Generic;

namespace VK.Blocks.AI.Cognitive;


/// <summary>
/// Represents an AI persona card.
/// </summary>
public sealed record VKPersonaCard
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
    /// Gets the description/personality of the persona.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the knowledge entries associated with the persona.
    /// </summary>
    public IEnumerable<VKKnowledgeEntry> Knowledge { get; init; } = [];

    /// <summary>
    /// Gets the evolving traits of the persona.
    /// </summary>
    public IDictionary<string, string> Traits { get; init; } = new Dictionary<string, string>();
}

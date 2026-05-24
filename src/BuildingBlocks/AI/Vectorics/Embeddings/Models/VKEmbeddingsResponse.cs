using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Represents the structured response from a vector embeddings engine.
/// </summary>
public sealed record VKEmbeddingsResponse
{
    /// <summary>
    /// Gets the list of generated embedding vectors.
    /// </summary>
    public required IReadOnlyList<VKEmbeddingsVector> Vectors { get; init; }

    /// <summary>
    /// Gets the identifier of the model that generated the embeddings.
    /// </summary>
    public string? ModelId { get; init; }

    /// <summary>
    /// Gets the token usage information for the generation.
    /// </summary>
    public VKAITokenUsage? Usage { get; init; }

    /// <summary>
    /// Gets additional metadata for the generation.
    /// </summary>
    public IReadOnlyDictionary<string, object?>? Metadata { get; init; }
}

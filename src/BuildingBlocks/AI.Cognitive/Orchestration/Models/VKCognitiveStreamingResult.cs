using System.Collections.Generic;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Represents a partial streaming chunk returned by the cognitive pipeline.
/// Follows AP.01 (Modern C# record).
/// </summary>
public sealed record VKCognitiveStreamingResult
{
    /// <summary>
    /// Gets the text delta of the assistant response.
    /// </summary>
    public string? ContentDelta { get; init; }

    /// <summary>
    /// Gets the partial reasoning/CoT thinking delta.
    /// </summary>
    public string? ReasoningDelta { get; init; }

    /// <summary>
    /// Gets the active intent identified during routing.
    /// </summary>
    public VKIntent? Intent { get; init; }

    /// <summary>
    /// Gets a value indicating whether this chunk represents the final emission.
    /// </summary>
    public bool IsFinal { get; init; }

    /// <summary>
    /// Gets the active emission phase: Weaving or Emitting.
    /// Conforms to COG03 (Server-Sent Events phase tagging).
    /// </summary>
    public string EmissionPhase { get; init; } = "Emitting";

    /// <summary>
    /// Gets the incremental token metrics or metadata.
    /// </summary>
    public IDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();
}

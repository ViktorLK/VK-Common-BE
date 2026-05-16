using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Represents the structured response from a text generation engine.
/// </summary>
public sealed record VKTextResponse
{
    /// <summary>
    /// Gets the generated text content.
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// Gets the identifier of the model that generated the text.
    /// </summary>
    public string? ModelId { get; init; }

    /// <summary>
    /// Gets the token usage information for the generation.
    /// </summary>
    public VKAIUsage? Usage { get; init; }

    /// <summary>
    /// Gets the reason why the generation finished (e.g., "stop", "length").
    /// </summary>
    public string? FinishReason { get; init; }

    /// <summary>
    /// Gets additional metadata for the generation.
    /// </summary>
    public IDictionary<string, object?>? Metadata { get; init; }
}

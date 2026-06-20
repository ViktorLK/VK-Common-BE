using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Represents a strongly-typed, structured chat response where the LLM output has been
/// deserialized into a concrete type using JSON Schema-constrained generation.
/// </summary>
/// <typeparam name="T">The type that the LLM response was deserialized into.</typeparam>
public sealed record VKStructuredChatResponse<T> where T : class
{
    /// <summary>
    /// Gets the deserialized response data from the LLM.
    /// </summary>
    public required T Data { get; init; }

    /// <summary>
    /// Gets the token usage information for the request.
    /// </summary>
    public VKAITokenUsage? Usage { get; init; }

    /// <summary>
    /// Gets the model identifier that generated the response.
    /// </summary>
    public string? ModelId { get; init; }

    /// <summary>
    /// Gets the finish reason from the provider (e.g., "stop", "length").
    /// </summary>
    public string? FinishReason { get; init; }

    /// <summary>
    /// Gets the raw metadata returned by the provider.
    /// </summary>
    public IReadOnlyDictionary<string, object?>? Metadata { get; init; }
}

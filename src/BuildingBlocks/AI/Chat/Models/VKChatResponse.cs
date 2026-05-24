using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Represents the structured response from a chat engine.
/// </summary>
public sealed record VKChatResponse
{
    /// <summary>
    /// Gets the assistant's message.
    /// </summary>
    public required VKChatMessage Message { get; init; }

    /// <summary>
    /// Gets the token usage information for the request.
    /// </summary>
    public VKAITokenUsage? Usage { get; init; }

    /// <summary>
    /// Gets the reason why the generation finished (e.g., "stop", "length").
    /// </summary>
    public string? FinishReason { get; init; }

    /// <summary>
    /// Gets additional metadata for the response.
    /// </summary>
    public IDictionary<string, object?>? Metadata { get; init; }
}

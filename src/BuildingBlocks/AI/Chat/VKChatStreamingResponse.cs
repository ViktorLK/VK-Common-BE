using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Represents a chunk of a streaming chat response.
/// </summary>
public sealed record VKChatStreamingResponse
{
    /// <summary>
    /// Gets the partial content of the message.
    /// </summary>
    public string Delta { get; init; } = string.Empty;

    /// <summary>
    /// Gets the role of the message (usually Assistant).
    /// </summary>
    public VKChatRole Role { get; init; } = VKChatRole.Assistant;

    /// <summary>
    /// Gets the identifier of the model that generated this message.
    /// </summary>
    public string? ModelId { get; init; }

    /// <summary>
    /// Gets the partial reasoning/thinking content of the message.
    /// </summary>
    public string? ReasoningDelta { get; init; }

    /// <summary>
    /// Gets a value indicating whether this is the final chunk.
    /// </summary>
    public bool IsFinal { get; init; }

    /// <summary>
    /// Gets additional metadata for the chunk.
    /// </summary>
    public IDictionary<string, object?>? Metadata { get; init; }
}

using System;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// A single pulse of conversation history representing an echo in short-term memory.
/// Follows AP.01 (sealed record for immutability).
/// </summary>
public sealed record VKEchoTrace : IVKFragmentMetadata
{
    /// <summary>
    /// Gets the session identifier that owns this conversation echo trace.
    /// </summary>
    public required VKSessionId SessionId { get; init; }

    /// <summary>
    /// Gets the unique message trace identifier for this echo entry.
    /// </summary>
    public required VKEchoId Id { get; init; }

    /// <summary>
    /// Gets the chat role (e.g., User, Assistant, System) for this dialogue turn.
    /// </summary>
    public required VKChatRole Role { get; init; }

    /// <summary>
    /// Gets the raw text content of the dialogue message.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Gets the estimated or calculated token count for this message content.
    /// </summary>
    public int TokenCount { get; init; }

    /// <summary>
    /// Gets the timestamp when this echo message was recorded.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

}

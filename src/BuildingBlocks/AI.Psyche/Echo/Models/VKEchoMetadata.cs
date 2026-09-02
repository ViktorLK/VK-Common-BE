using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Lightweight metadata for a conversation echo trace excluding the heavy text content payload.
/// Used for fast token budget calculations and sliding-window evaluations without full text deserialization.
/// Follows AP.01 (sealed record for immutability).
/// </summary>
public sealed record VKEchoMetadata
{
    /// <summary>
    /// Gets the unique message trace identifier for this echo entry.
    /// </summary>
    public required VKEchoId Id { get; init; }

    /// <summary>
    /// Gets the session identifier that owns this conversation echo trace.
    /// </summary>
    public required VKSessionId SessionId { get; init; }

    /// <summary>
    /// Gets the chat role (e.g., User, Assistant, System) for this dialogue turn.
    /// </summary>
    public required VKChatRole Role { get; init; }

    /// <summary>
    /// Gets the estimated or calculated token count for this message.
    /// </summary>
    public int TokenCount { get; init; }

    /// <summary>
    /// Gets the timestamp when this echo message was recorded.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }
}

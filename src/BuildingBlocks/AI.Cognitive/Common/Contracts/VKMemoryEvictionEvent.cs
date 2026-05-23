using System;
using System.Collections.Generic;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Domain/integration event representing the eviction of messages from a conversation's working memory presence.
/// Follows AP.01 (sealed record, required properties) and AP.03.
/// </summary>
public sealed record VKMemoryEvictionEvent
{
    /// <summary>
    /// Gets the unique session identifier.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// Gets the tenant identifier for strict isolation.
    /// </summary>
    public required string TenantId { get; init; }

    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// Gets the chronological list of conversation messages that were evicted from presence.
    /// </summary>
    public required IReadOnlyList<VKChatMessage> EvictedMessages { get; init; }

    /// <summary>
    /// Gets the timestamp when this eviction occurred.
    /// </summary>
    public required DateTimeOffset OccurredAt { get; init; }
}

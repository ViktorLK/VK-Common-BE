using System;
using System.Collections.Immutable;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cortex;

/// <summary>
/// Immutable correlation context carrier threading TraceId, SessionId, and metadata across AI BuildingBlocks.
/// Pure runtime carrier focused on dialogue execution identity.
/// Follows [AP.01] (sealed record).
/// </summary>
public sealed record VKCortexCorrelationContext
{
    /// <summary>
    /// Gets the unique distributed trace identifier for the current turn.
    /// </summary>
    public required string TraceId { get; init; }

    /// <summary>
    /// Gets the session identifier associated with this correlation scope.
    /// </summary>
    public required VKSessionId SessionId { get; init; }

    /// <summary>
    /// Gets the timestamp when this correlation context was initiated.
    /// </summary>
    public DateTimeOffset InitiatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets additional key-value baggage items for cross-block observability.
    /// </summary>
    public ImmutableDictionary<string, string> Baggage { get; init; } = ImmutableDictionary<string, string>.Empty;

    /// <summary>
    /// Creates a new correlation context with the specified trace and session identifiers.
    /// </summary>
    public static VKCortexCorrelationContext Create(string traceId, VKSessionId sessionId)
    {
        VKGuard.NotNullOrWhiteSpace(traceId);
        VKGuard.NotDefault(sessionId);
        return new VKCortexCorrelationContext
        {
            TraceId = traceId,
            SessionId = sessionId
        };
    }
}

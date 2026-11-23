using System;

namespace VK.Blocks.AI;

/// <summary>
/// Represents the observed outcome of an AI provider invocation.
/// </summary>
// [AP.01]
public sealed record VKAIInvocationOutcome
{
    /// <summary>
    /// Gets the unique invocation identifier.
    /// </summary>
    public required string InvocationId { get; init; }

    /// <summary>
    /// Gets the 1-based attempt number within the logical invocation.
    /// </summary>
    public int AttemptNo { get; init; } = 1;

    /// <summary>
    /// Gets the connection identifier used for the invocation.
    /// </summary>
    public required string ConnectionId { get; init; }

    /// <summary>
    /// Gets a value indicating whether the invocation succeeded.
    /// </summary>
    public required bool IsSuccess { get; init; }

    /// <summary>
    /// Gets the failure kind when <see cref="IsSuccess"/> is <see langword="false"/>.
    /// </summary>
    public VKAIFailureKind FailureKind { get; init; }

    /// <summary>
    /// Gets the optional retry-after duration suggested by the provider.
    /// </summary>
    public TimeSpan? RetryAfter { get; init; }

    /// <summary>
    /// Gets the optional number of input tokens consumed.
    /// </summary>
    public int? InputTokens { get; init; }

    /// <summary>
    /// Gets the optional number of output tokens generated.
    /// </summary>
    public int? OutputTokens { get; init; }

    /// <summary>
    /// Gets the optional number of cached tokens used.
    /// </summary>
    public int? CachedTokens { get; init; }

    /// <summary>
    /// Gets the optional number of reasoning tokens consumed.
    /// </summary>
    public int? ReasoningTokens { get; init; }

    /// <summary>
    /// Gets optional correlation tags for observability.
    /// </summary>
    public VKAICorrelationTags? CorrelationTags { get; init; }

    /// <summary>
    /// Gets a value indicating whether the invocation produced tool side effects.
    /// </summary>
    public bool HasToolSideEffects { get; init; }

    /// <summary>
    /// Gets the optional total duration of the invocation.
    /// </summary>
    public TimeSpan? Duration { get; init; }

    /// <summary>
    /// Gets the optional time to first token for streaming invocations.
    /// </summary>
    public TimeSpan? TimeToFirstToken { get; init; }

    /// <summary>
    /// Gets the optional scope key associated with the invocation.
    /// </summary>
    public VKAIScopeKey? ScopeKey { get; init; }

    /// <summary>
    /// Gets the optional HTTP status code returned by the provider.
    /// </summary>
    public int? HttpStatusCode { get; init; }

    /// <summary>
    /// Gets the disposition of a failed invocation for retry and failover decisions.
    /// </summary>
    public VKAIFailureDisposition FailureDisposition { get; init; }

    /// <summary>
    /// Gets a value indicating whether at least one streaming output chunk was committed before termination.
    /// </summary>
    public bool FirstChunkCommitted { get; init; }

    /// <summary>
    /// Gets the optional capacity or token reservation identifier associated with the invocation.
    /// </summary>
    public string? ReservationId { get; init; }

    /// <summary>
    /// Gets the optional affinity key used for sticky routing pin/unpin decisions.
    /// </summary>
    public string? AffinityKey { get; init; }
}

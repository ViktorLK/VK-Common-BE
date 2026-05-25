using System;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Represents the situational and operational presence state (Working Memory & Context Window bounds) of the Agent.
/// Follows AP.01 (sealed record with required properties) and AP.03.
/// </summary>
public sealed record VKPresenceState
{
    /// <summary>
    /// Gets the session identifier.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// Gets the tenant identifier.
    /// </summary>
    public string? TenantId { get; init; }

    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// Gets the environmental label (e.g. "Development", "Staging", "Production").
    /// </summary>
    public string? Environment { get; init; }

    /// <summary>
    /// Gets the current active pipeline execution stage (e.g. "Reasoning", "Planning", "Execution").
    /// </summary>
    public string? PipelineStage { get; init; }

    /// <summary>
    /// Gets the maximum request-level token quota limit.
    /// </summary>
    public required int MaxRequestTokenQuota { get; init; }

    /// <summary>
    /// Gets the safety token margin buffer before triggering absolute window clipping.
    /// </summary>
    public required int SafetyMarginTokens { get; init; }

    /// <summary>
    /// Gets the current temporal presence (timestamp).
    /// </summary>
    public required DateTimeOffset CurrentTime { get; init; }

    /// <summary>
    /// Gets a value indicating whether the current time is within business hours (9:00 - 18:00 default).
    /// </summary>
    public required bool IsBusinessHours { get; init; }

    /// <summary>
    /// Gets the active day of week.
    /// </summary>
    public required DayOfWeek DayOfWeek { get; init; }

    /// <summary>
    /// Gets the accumulated prompt tokens used so far in this session.
    /// </summary>
    public required int TotalPromptTokensUsed { get; init; }

    /// <summary>
    /// Gets the accumulated completion tokens used so far in this session.
    /// </summary>
    public required int TotalCompletionTokensUsed { get; init; }

    /// <summary>
    /// Gets the total tokens used (prompt + completion).
    /// </summary>
    public int TotalTokensUsed => TotalPromptTokensUsed + TotalCompletionTokensUsed;

    /// <summary>
    /// Gets the estimated active message count in working memory.
    /// </summary>
    public required int ActiveMessageCount { get; init; }

    /// <summary>
    /// Gets the remaining token budget before warning/truncation threshold.
    /// </summary>
    public required int RemainingTokenBudget { get; init; }

    /// <summary>
    /// Gets the recent input text delta.
    /// </summary>
    public string? RecentInput { get; init; }

    /// <summary>
    /// Gets the environmental world state (geographic location, user activity, ambient tags).
    /// </summary>
    public required VKWorldState WorldState { get; init; }
}

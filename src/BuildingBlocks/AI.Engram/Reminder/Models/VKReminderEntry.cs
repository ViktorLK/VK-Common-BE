using System;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Represents a prospective memory entry to be fired under specific conditions.
/// </summary>
public sealed record VKReminderEntry
{
    // // [AP.01] sealed record and required properties
    /// <summary>
    /// Gets the unique identifier for the reminder.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the session identifier this reminder belongs to.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// Gets the trigger condition type.
    /// </summary>
    public required VKReminderTriggerType TriggerType { get; init; }

    /// <summary>
    /// Gets the raw string representation of the trigger condition (e.g. absolute ISO time, topic query).
    /// </summary>
    public required string TriggerCondition { get; init; }

    /// <summary>
    /// Gets the content payload to be presented when the reminder triggers.
    /// </summary>
    public required string PayloadContent { get; init; }

    /// <summary>
    /// Gets the timestamp when this reminder was created.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Gets the original due date/time if calculated at creation.
    /// </summary>
    public DateTimeOffset? OriginalDueAt { get; init; }

    /// <summary>
    /// Gets the optional expiration timestamp.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>
    /// Gets the timestamp until which this reminder is snoozed.
    /// </summary>
    public DateTimeOffset? SnoozedUntil { get; init; }

    /// <summary>
    /// Gets the count of times this reminder has been snoozed.
    /// </summary>
    public int SnoozeCount { get; init; }

    /// <summary>
    /// Gets the timestamp when this reminder was fired.
    /// </summary>
    public DateTimeOffset? FiredAt { get; init; }

    /// <summary>
    /// Gets the version stamp for concurrency control.
    /// </summary>
    public long Version { get; init; } = 1;

    /// <summary>
    /// Gets the status of the reminder.
    /// </summary>
    public VKReminderStatus Status { get; init; } = VKReminderStatus.Pending;
}

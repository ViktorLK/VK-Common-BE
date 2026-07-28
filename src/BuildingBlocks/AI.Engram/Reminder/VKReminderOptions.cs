using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Configuration options for the Prospective Memory (Reminder) subsystem.
/// </summary>
public sealed partial record VKReminderOptions : IVKToggleableBlockOptions
{
    // // [AP.01] sealed record and options isolation
    /// <summary>
    /// Gets a value indicating whether the Reminder subsystem is enabled.
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// Gets the background scan interval for time-based reminders in seconds.
    /// </summary>
    public double ScanIntervalSeconds { get; init; } = 30;

    /// <summary>
    /// Gets the default time-to-live for reminders in days.
    /// </summary>
    public int DefaultExpiryDays { get; init; } = 30;

    /// <summary>
    /// Gets the minimum similarity threshold for semantic topic matching.
    /// </summary>
    public float TopicSimilarityThreshold { get; init; } = 0.6f;

    /// <summary>
    /// Gets the presentation mode for fired reminders.
    /// </summary>
    public VKReminderPresentationMode PresentationMode { get; init; } = VKReminderPresentationMode.InjectIntoContext;

    /// <summary>
    /// Gets the policy handling overdue/missed reminders on system start.
    /// </summary>
    public VKMissedReminderPolicy MissedPolicy { get; init; } = VKMissedReminderPolicy.FireAll;

    /// <summary>
    /// Gets the maximum allowed snooze count for a reminder.
    /// </summary>
    public int MaxSnoozeCount { get; init; } = 3;

    /// <summary>
    /// Gets the default snooze duration in minutes.
    /// </summary>
    public double DefaultSnoozeDurationMinutes { get; init; } = 60;

    /// <summary>
    /// Gets the prompt fragment depth priority when injecting reminders into Psyche context.
    /// </summary>
    public int ReminderDepthPriority { get; init; } = 600;
}

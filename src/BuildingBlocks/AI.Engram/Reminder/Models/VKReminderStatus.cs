namespace VK.Blocks.AI.Engram;

/// <summary>
/// Represents the execution status of a prospective reminder.
/// </summary>
public enum VKReminderStatus
{
    /// <summary>
    /// Reminder is pending trigger conditions.
    /// </summary>
    Pending,

    /// <summary>
    /// Reminder is currently snoozed/postponed.
    /// </summary>
    Snoozed,

    /// <summary>
    /// Reminder has been successfully fired and executed.
    /// </summary>
    Fired,

    /// <summary>
    /// Reminder was overdue while system was offline and fired upon recovery.
    /// </summary>
    MissedFired,

    /// <summary>
    /// Reminder expired before its trigger conditions were met.
    /// </summary>
    Expired,

    /// <summary>
    /// Reminder was overdue while system was offline and expired per policy.
    /// </summary>
    MissedExpired,

    /// <summary>
    /// Reminder was manually cancelled.
    /// </summary>
    Cancelled
}

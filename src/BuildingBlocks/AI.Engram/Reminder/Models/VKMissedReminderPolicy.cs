namespace VK.Blocks.AI.Engram;

/// <summary>
/// Policy governing how overdue/missed reminders are handled upon application startup or recovery.
/// </summary>
public enum VKMissedReminderPolicy
{
    /// <summary>
    /// Fire all overdue reminders.
    /// </summary>
    FireAll,

    /// <summary>
    /// Fire only the single latest overdue reminder per session and expire the rest.
    /// </summary>
    FireLatestOnly,

    /// <summary>
    /// Skip and mark all overdue reminders as expired.
    /// </summary>
    Skip
}

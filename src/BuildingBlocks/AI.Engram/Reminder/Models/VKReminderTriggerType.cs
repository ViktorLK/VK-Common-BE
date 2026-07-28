namespace VK.Blocks.AI.Engram;

/// <summary>
/// Defines the trigger type for prospective reminders.
/// </summary>
public enum VKReminderTriggerType
{
    /// <summary>
    /// Triggered at an absolute time.
    /// </summary>
    AtTime,

    /// <summary>
    /// Triggered at a relative duration from creation.
    /// </summary>
    AtRelativeTime,

    /// <summary>
    /// Triggered when a new session starts.
    /// </summary>
    OnSessionStart,

    /// <summary>
    /// Triggered when conversation matches a specific topic.
    /// </summary>
    OnTopicMatch,

    /// <summary>
    /// Triggered by an external event.
    /// </summary>
    OnEventTrigger
}

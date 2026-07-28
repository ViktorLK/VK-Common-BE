namespace VK.Blocks.AI.Engram.Reminder.Diagnostics;

/// <summary>
/// Diagnostic constants for the Reminder slice.
/// Follows BB.04.
/// </summary>
public static class VKReminderDiagnosticsConstants
{
    // Service-level events
    public const int ReminderSavedEventId = 501;
    public const int ReminderFiredEventId = 502;
    public const int ReminderCancelledEventId = 503;
    public const int ReminderSnoozedEventId = 504;
    public const int ReminderExpiredEventId = 505;
    public const int TopicMatchEvaluatedEventId = 506;
    public const int MissedReminderHandledEventId = 507;

    // Background worker events
    public const int WorkerDisabledEventId = 511;
    public const int WorkerStartedEventId = 512;
    public const int WorkerStoppedEventId = 513;
    public const int ScanErrorEventId = 514;
    public const int FetchFailedEventId = 515;
    public const int TimeFiredEventId = 516;
    public const int MissedCompensationStartedEventId = 517;
}

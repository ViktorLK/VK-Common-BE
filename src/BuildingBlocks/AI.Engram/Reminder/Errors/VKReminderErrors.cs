using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Error constants for the Reminder (Prospective Memory) subsystem.
/// Follows CS.01.
/// </summary>
public static class VKReminderErrors
{
    public static readonly VKError NotFound = new("AI.Engram.Reminder.NotFound", "The specified reminder was not found.");
    public static readonly VKError AlreadyFired = new("AI.Engram.Reminder.AlreadyFired", "Cannot modify a reminder that has already been fired.");
    public static readonly VKError AlreadyCancelled = new("AI.Engram.Reminder.AlreadyCancelled", "Cannot modify a reminder that has been cancelled.");
    public static readonly VKError AlreadyExpired = new("AI.Engram.Reminder.AlreadyExpired", "Cannot modify a reminder that has expired.");
    public static readonly VKError MaxSnoozeExceeded = new("AI.Engram.Reminder.MaxSnoozeExceeded", "Maximum snooze count has been exceeded for this reminder.");
    public static readonly VKError ConcurrencyConflict = new("AI.Engram.Reminder.ConcurrencyConflict", "Reminder was modified by another process (version mismatch).");
    public static readonly VKError LlmEvaluationFailed = new("AI.Engram.Reminder.LlmEvaluationFailed", "LLM topic match evaluation failed.");
    public static readonly VKError EventReceiverNotConfigured = new("AI.Engram.Reminder.EventReceiverNotConfigured", "No event receiver has been configured for external event triggers.");
}

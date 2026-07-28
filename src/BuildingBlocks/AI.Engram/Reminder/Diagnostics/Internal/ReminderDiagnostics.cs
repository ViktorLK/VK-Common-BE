using System;
using Microsoft.Extensions.Logging;

namespace VK.Blocks.AI.Engram.Reminder.Diagnostics.Internal;

/// <summary>
/// Structured LoggerMessage definitions for the Reminder slice.
/// Follows OR.01.
/// </summary>
internal static partial class ReminderDiagnostics
{
    [LoggerMessage(EventId = VKReminderDiagnosticsConstants.ReminderSavedEventId, Level = LogLevel.Information, Message = "Reminder {Id} saved for session {SessionId} with trigger type {TriggerType}.")]
    public static partial void ReminderSaved(this ILogger logger, string id, string sessionId, string triggerType);

    [LoggerMessage(EventId = VKReminderDiagnosticsConstants.ReminderFiredEventId, Level = LogLevel.Information, Message = "Reminder {Id} fired for session {SessionId}. Presentation mode: {PresentationMode}.")]
    public static partial void ReminderFired(this ILogger logger, string id, string sessionId, string presentationMode);

    [LoggerMessage(EventId = VKReminderDiagnosticsConstants.ReminderCancelledEventId, Level = LogLevel.Information, Message = "Reminder {Id} cancelled.")]
    public static partial void ReminderCancelled(this ILogger logger, string id);

    [LoggerMessage(EventId = VKReminderDiagnosticsConstants.ReminderSnoozedEventId, Level = LogLevel.Information, Message = "Reminder {Id} snoozed until {SnoozedUntil} (Snooze count: {SnoozeCount}).")]
    public static partial void ReminderSnoozed(this ILogger logger, string id, DateTimeOffset snoozedUntil, int snoozeCount);

    [LoggerMessage(EventId = VKReminderDiagnosticsConstants.ReminderExpiredEventId, Level = LogLevel.Information, Message = "Reminder {Id} marked as expired.")]
    public static partial void ReminderExpired(this ILogger logger, string id);

    [LoggerMessage(EventId = VKReminderDiagnosticsConstants.TopicMatchEvaluatedEventId, Level = LogLevel.Debug, Message = "Topic match evaluated for reminder {Id}. Result: {Matched}.")]
    public static partial void TopicMatchEvaluated(this ILogger logger, string id, bool matched);

    [LoggerMessage(EventId = VKReminderDiagnosticsConstants.MissedReminderHandledEventId, Level = LogLevel.Information, Message = "Missed reminder {Id} processed with action {Status}.")]
    public static partial void MissedReminderHandled(this ILogger logger, string id, string status);

    [LoggerMessage(EventId = VKReminderDiagnosticsConstants.WorkerDisabledEventId, Level = LogLevel.Information, Message = "Reminder background scanner is disabled.")]
    public static partial void WorkerDisabled(this ILogger logger);

    [LoggerMessage(EventId = VKReminderDiagnosticsConstants.WorkerStartedEventId, Level = LogLevel.Information, Message = "Reminder background scanner started. Polling every {IntervalSeconds} seconds.")]
    public static partial void WorkerStarted(this ILogger logger, double intervalSeconds);

    [LoggerMessage(EventId = VKReminderDiagnosticsConstants.WorkerStoppedEventId, Level = LogLevel.Information, Message = "Reminder background scanner stopped.")]
    public static partial void WorkerStopped(this ILogger logger);

    [LoggerMessage(EventId = VKReminderDiagnosticsConstants.ScanErrorEventId, Level = LogLevel.Error, Message = "An error occurred during reminder background scanning.")]
    public static partial void ScanError(this ILogger logger, Exception ex);

    [LoggerMessage(EventId = VKReminderDiagnosticsConstants.FetchFailedEventId, Level = LogLevel.Error, Message = "Failed to fetch pending reminders from store: {Errors}")]
    public static partial void FetchFailed(this ILogger logger, string errors);

    [LoggerMessage(EventId = VKReminderDiagnosticsConstants.TimeFiredEventId, Level = LogLevel.Information, Message = "Time-based reminder {Id} fired for session {SessionId} via background worker.")]
    public static partial void TimeFired(this ILogger logger, string id, string sessionId);

    [LoggerMessage(EventId = VKReminderDiagnosticsConstants.MissedCompensationStartedEventId, Level = LogLevel.Information, Message = "Starting missed reminder compensation sweep. Policy: {Policy}.")]
    public static partial void MissedCompensationStarted(this ILogger logger, string policy);
}

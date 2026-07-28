using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Interface defining operations for the Reminder orchestration service.
/// </summary>
public interface IVKReminderService
{
    // // [CS.03] Async + CancellationToken + [CS.01] Result Pattern
    /// <summary>
    /// Saves a prospective reminder.
    /// </summary>
    Task<VKResult> SaveReminderAsync(VKReminderEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a pending or snoozed reminder.
    /// </summary>
    Task<VKResult> CancelReminderAsync(string reminderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Snoozes a reminder by the specified duration or default option duration.
    /// </summary>
    Task<VKResult> SnoozeReminderAsync(string reminderId, System.TimeSpan? snoozeDuration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all pending reminders for a given session.
    /// </summary>
    Task<VKResult<IReadOnlyList<VKReminderEntry>>> GetPendingRemindersAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates pending reminders against the current conversation context (turn) and fires matching reminders.
    /// </summary>
    Task<VKResult> EvaluateRemindersAsync(VKPsycheContext context, CancellationToken cancellationToken = default);
}

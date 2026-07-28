using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Interface for managing the persistence of prospective reminder entries.
/// </summary>
public interface IVKReminderStore
{
    // // [CS.03] Async + CancellationToken + [CS.01] Result Pattern
    /// <summary>
    /// Saves a reminder entry.
    /// </summary>
    Task<VKResult> SaveAsync(VKReminderEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a reminder by its identifier.
    /// </summary>
    Task<VKResult<VKReminderEntry>> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all pending or snoozed reminders for a given session.
    /// </summary>
    Task<VKResult<IReadOnlyList<VKReminderEntry>>> GetPendingAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all pending or snoozed reminders across all sessions.
    /// </summary>
    Task<VKResult<IReadOnlyList<VKReminderEntry>>> GetAllPendingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all reminders due at or before the specified timestamp.
    /// </summary>
    Task<VKResult<IReadOnlyList<VKReminderEntry>>> GetDueAsync(DateTimeOffset asOf, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing reminder entry with version checking.
    /// </summary>
    Task<VKResult> UpdateAsync(VKReminderEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the status of a specific reminder entry.
    /// </summary>
    Task<VKResult> UpdateStatusAsync(string id, VKReminderStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a reminder entry by identifier.
    /// </summary>
    Task<VKResult> DeleteAsync(string id, CancellationToken cancellationToken = default);
}

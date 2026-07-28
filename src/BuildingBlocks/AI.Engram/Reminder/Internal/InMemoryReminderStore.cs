using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Reminder.Internal;

// // [AP.01] sealed class default and [AP.03] Basic prefix for in-memory single-node stores
internal sealed class InMemoryReminderStore : IVKReminderStore
{
    private readonly ConcurrentDictionary<string, VKReminderEntry> _reminders = new();

    public Task<VKResult> SaveAsync(VKReminderEntry entry, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // // [AP.01] Boundary check via VKGuard
        VKGuard.NotNull(entry);

        _reminders[entry.Id] = entry;
        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult<VKReminderEntry>> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNullOrWhiteSpace(id);

        if (_reminders.TryGetValue(id, out var entry))
        {
            return Task.FromResult(VKResult.Success(entry));
        }

        return Task.FromResult(VKResult.Failure<VKReminderEntry>(VKReminderErrors.NotFound));
    }

    public Task<VKResult<IReadOnlyList<VKReminderEntry>>> GetPendingAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNullOrWhiteSpace(sessionId);

        IReadOnlyList<VKReminderEntry> list = _reminders.Values
            .Where(r => r.SessionId == sessionId && (r.Status == VKReminderStatus.Pending || r.Status == VKReminderStatus.Snoozed))
            .ToList();

        return Task.FromResult(VKResult.Success(list));
    }

    public Task<VKResult<IReadOnlyList<VKReminderEntry>>> GetAllPendingAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<VKReminderEntry> list = _reminders.Values
            .Where(r => r.Status == VKReminderStatus.Pending || r.Status == VKReminderStatus.Snoozed)
            .ToList();

        return Task.FromResult(VKResult.Success(list));
    }

    public Task<VKResult<IReadOnlyList<VKReminderEntry>>> GetDueAsync(DateTimeOffset asOf, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<VKReminderEntry> list = _reminders.Values
            .Where(r => r.Status == VKReminderStatus.Pending || r.Status == VKReminderStatus.Snoozed)
            .Where(r =>
                (r.SnoozedUntil.HasValue && r.SnoozedUntil.Value <= asOf) ||
                (!r.SnoozedUntil.HasValue && r.ExpiresAt.HasValue && r.ExpiresAt.Value <= asOf) ||
                (!r.SnoozedUntil.HasValue && r.OriginalDueAt.HasValue && r.OriginalDueAt.Value <= asOf))
            .ToList();

        return Task.FromResult(VKResult.Success(list));
    }

    public Task<VKResult> UpdateAsync(VKReminderEntry entry, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNull(entry);

        if (!_reminders.TryGetValue(entry.Id, out var existing))
        {
            return Task.FromResult(VKResult.Failure(VKReminderErrors.NotFound));
        }

        // Optimistic concurrency version check
        if (existing.Version != entry.Version)
        {
            return Task.FromResult(VKResult.Failure(VKReminderErrors.ConcurrencyConflict));
        }

        var nextEntry = entry with { Version = existing.Version + 1 };
        _reminders[entry.Id] = nextEntry;
        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult> UpdateStatusAsync(string id, VKReminderStatus status, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNullOrWhiteSpace(id);

        if (_reminders.TryGetValue(id, out var entry))
        {
            _reminders[id] = entry with { Status = status, Version = entry.Version + 1 };
            return Task.FromResult(VKResult.Success());
        }

        return Task.FromResult(VKResult.Failure(VKReminderErrors.NotFound));
    }

    public Task<VKResult> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNullOrWhiteSpace(id);

        if (_reminders.TryRemove(id, out _))
        {
            return Task.FromResult(VKResult.Success());
        }

        return Task.FromResult(VKResult.Failure(VKReminderErrors.NotFound));
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Engram.Reminder.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Reminder.Internal;

// // [AP.01] sealed class default
internal sealed class ReminderScanBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;
    private readonly IVKGuidGenerator _guidGenerator;
    private readonly IVKDistributedLockProvider _lockProvider;
    private readonly IVKActiveTenantProvider _activeTenantProvider;
    private readonly VKReminderOptions _options;
    private readonly ILogger<ReminderScanBackgroundService> _logger;

    public ReminderScanBackgroundService(
        IServiceScopeFactory scopeFactory,
        TimeProvider timeProvider,
        IVKGuidGenerator guidGenerator,
        IVKDistributedLockProvider lockProvider,
        IVKActiveTenantProvider activeTenantProvider,
        IOptions<VKReminderOptions> options,
        ILogger<ReminderScanBackgroundService> logger)
    {
        // // [AP.01] Fluent guard assignment
        _scopeFactory = VKGuard.NotNull(scopeFactory);
        _timeProvider = VKGuard.NotNull(timeProvider);
        _guidGenerator = VKGuard.NotNull(guidGenerator);
        _lockProvider = VKGuard.NotNull(lockProvider);
        _activeTenantProvider = VKGuard.NotNull(activeTenantProvider);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.WorkerDisabled();
            return;
        }

        _logger.WorkerStarted(_options.ScanIntervalSeconds);

        // Perform initial startup compensation for missed reminders during offline/shutdown period
        await CompensateMissedRemindersAsync(stoppingToken).ConfigureAwait(false);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // // [CS.03] Async delay with ConfigureAwait(false)
                await Task.Delay(TimeSpan.FromSeconds(_options.ScanIntervalSeconds), stoppingToken).ConfigureAwait(false);

                // Distributed Coordination: Ensure only single node scans at a time
                await using var lockHandle = await _lockProvider.TryAcquireLockAsync(
                    "engram:jobs:reminder_scan",
                    TimeSpan.FromSeconds(_options.ScanIntervalSeconds * 2),
                    stoppingToken).ConfigureAwait(false);

                if (lockHandle is not null && lockHandle.IsAcquired)
                {
                    await ScanAndFireRemindersAsync(stoppingToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.ScanError(ex);
            }
        }

        _logger.WorkerStopped();
    }

    private async Task CompensateMissedRemindersAsync(CancellationToken cancellationToken)
    {
        _logger.MissedCompensationStarted(_options.MissedPolicy.ToString());

        var tenantsResult = await _activeTenantProvider.GetActiveTenantsAsync(cancellationToken).ConfigureAwait(false);
        if (tenantsResult.IsFailure)
        {
            _logger.FetchFailed(string.Join("; ", tenantsResult.Errors.Select(e => e.Description)));
            return;
        }

        foreach (var tenantId in tenantsResult.Value)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            await CompensateMissedRemindersForTenantAsync(tenantId, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task CompensateMissedRemindersForTenantAsync(VKTenantId tenantId, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var tenantSetter = scope.ServiceProvider.GetService<IVKTenantSetter>();
        tenantSetter?.SetCurrentTenantId(tenantId);

        var reminderStore = scope.ServiceProvider.GetRequiredService<IVKReminderStore>();
        var memoryStore = scope.ServiceProvider.GetRequiredService<IVKMemoryStore>();

        var now = _timeProvider.GetUtcNow();
        var dueResult = await reminderStore.GetDueAsync(now, cancellationToken).ConfigureAwait(false);
        if (dueResult.IsFailure)
        {
            _logger.FetchFailed(string.Join("; ", dueResult.Errors.Select(e => e.Description)));
            return;
        }

        var overdueReminders = dueResult.Value;
        if (overdueReminders.Count == 0)
        {
            return;
        }

        if (_options.MissedPolicy == VKMissedReminderPolicy.Skip)
        {
            foreach (var entry in overdueReminders)
            {
                await reminderStore.UpdateStatusAsync(entry.Id, VKReminderStatus.MissedExpired, cancellationToken).ConfigureAwait(false);
                _logger.MissedReminderHandled(entry.Id, VKReminderStatus.MissedExpired.ToString());
            }
            return;
        }

        if (_options.MissedPolicy == VKMissedReminderPolicy.FireAll)
        {
            foreach (var entry in overdueReminders)
            {
                await FireMissedReminderAsync(entry, reminderStore, memoryStore, now, cancellationToken).ConfigureAwait(false);
            }
        }
        else if (_options.MissedPolicy == VKMissedReminderPolicy.FireLatestOnly)
        {
            var groupedBySession = overdueReminders.GroupBy(r => r.SessionId);
            foreach (var group in groupedBySession)
            {
                var sorted = group.OrderByDescending(r => r.CreatedAt).ToList();
                var latest = sorted.First();

                await FireMissedReminderAsync(latest, reminderStore, memoryStore, now, cancellationToken).ConfigureAwait(false);

                foreach (var entry in sorted.Skip(1))
                {
                    await reminderStore.UpdateStatusAsync(entry.Id, VKReminderStatus.MissedExpired, cancellationToken).ConfigureAwait(false);
                    _logger.MissedReminderHandled(entry.Id, VKReminderStatus.MissedExpired.ToString());
                }
            }
        }
    }

    private async Task FireMissedReminderAsync(
        VKReminderEntry entry,
        IVKReminderStore reminderStore,
        IVKMemoryStore memoryStore,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        await reminderStore.UpdateStatusAsync(entry.Id, VKReminderStatus.MissedFired, cancellationToken).ConfigureAwait(false);
        _logger.MissedReminderHandled(entry.Id, VKReminderStatus.MissedFired.ToString());

        string displayContent = $"[Missed Reminder Fired] Reminder from previous session: {entry.PayloadContent}";

        Guid memoryGuid = Guid.TryParse(entry.Id, out var parsedGuid) ? parsedGuid : _guidGenerator.Create();

        var memoryEntry = new VKMemoryEntry
        {
            Id = new VKMemoryId(memoryGuid),
            Content = displayContent,
            CreatedAt = now,
            Category = VKMemoryCategory.ShortTerm,
            Importance = 0.5f,
            Metadata = new Dictionary<string, string>
            {
                ["SessionId"] = entry.SessionId,
                ["ReminderFired"] = "true",
                ["MissedCompensation"] = "true"
            }
        };

        await memoryStore.UpsertAsync(memoryEntry, cancellationToken).ConfigureAwait(false);
    }

    private async Task ScanAndFireRemindersAsync(CancellationToken cancellationToken)
    {
        var tenantsResult = await _activeTenantProvider.GetActiveTenantsAsync(cancellationToken).ConfigureAwait(false);
        if (tenantsResult.IsFailure)
        {
            _logger.FetchFailed(string.Join("; ", tenantsResult.Errors.Select(e => e.Description)));
            return;
        }

        foreach (var tenantId in tenantsResult.Value)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            await ScanAndFireRemindersForTenantAsync(tenantId, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task ScanAndFireRemindersForTenantAsync(VKTenantId tenantId, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var tenantSetter = scope.ServiceProvider.GetService<IVKTenantSetter>();
        tenantSetter?.SetCurrentTenantId(tenantId);

        var reminderStore = scope.ServiceProvider.GetRequiredService<IVKReminderStore>();
        var memoryStore = scope.ServiceProvider.GetRequiredService<IVKMemoryStore>();

        var pendingResult = await reminderStore.GetAllPendingAsync(cancellationToken).ConfigureAwait(false);
        if (pendingResult.IsFailure)
        {
            _logger.FetchFailed(string.Join("; ", pendingResult.Errors.Select(e => e.Description)));
            return;
        }

        var now = _timeProvider.GetUtcNow();

        foreach (var entry in pendingResult.Value)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            // Skip if snoozed in future
            if (entry.Status == VKReminderStatus.Snoozed && entry.SnoozedUntil.HasValue && entry.SnoozedUntil.Value > now)
            {
                continue;
            }

            // Check Expiration
            if (entry.ExpiresAt.HasValue && entry.ExpiresAt.Value < now)
            {
                await reminderStore.UpdateStatusAsync(entry.Id, VKReminderStatus.Expired, cancellationToken).ConfigureAwait(false);
                _logger.MissedReminderHandled(entry.Id, VKReminderStatus.Expired.ToString());
                continue;
            }

            bool shouldTrigger = false;

            if (entry.TriggerType == VKReminderTriggerType.AtTime)
            {
                if (DateTimeOffset.TryParse(entry.TriggerCondition, out var targetTime) && now >= targetTime)
                {
                    shouldTrigger = true;
                }
            }
            else if (entry.TriggerType == VKReminderTriggerType.AtRelativeTime)
            {
                if (TimeSpan.TryParse(entry.TriggerCondition, out var offset) && now >= entry.CreatedAt.Add(offset))
                {
                    shouldTrigger = true;
                }
            }

            if (shouldTrigger)
            {
                await reminderStore.UpdateStatusAsync(entry.Id, VKReminderStatus.Fired, cancellationToken).ConfigureAwait(false);
                _logger.TimeFired(entry.Id, entry.SessionId);

                string displayContent = $"[Reminder Fired] Last time, you asked to be reminded of: {entry.PayloadContent}";

                Guid memoryGuid = Guid.TryParse(entry.Id, out var parsedGuid) ? parsedGuid : _guidGenerator.Create();

                var memoryEntry = new VKMemoryEntry
                {
                    Id = new VKMemoryId(memoryGuid),
                    Content = displayContent,
                    CreatedAt = now,
                    Category = VKMemoryCategory.ShortTerm,
                    Importance = 0.5f,
                    Metadata = new Dictionary<string, string>
                    {
                        ["SessionId"] = entry.SessionId,
                        ["ReminderFired"] = "true"
                    }
                };

                await memoryStore.UpsertAsync(memoryEntry, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche;
using VK.Blocks.AI.Psyche.EFCore.Session.Internal;
using VK.Blocks.Core;
using VK.Blocks.Persistence;

namespace VK.Blocks.AI.Psyche.EFCore;

/// <summary>
/// Custom domain query extensions for <see cref="PsycheSessionRepository"/>.
/// Follows CS.01 (VKResult), CS.03 (ConfigureAwait(false)), CS.04 (Bounded pagination), and OR.01 (Structured logging).
/// </summary>
internal sealed partial class PsycheSessionRepository
{
    public async Task<VKResult<IReadOnlyList<VKSessionThread>>> ListActiveSessionsAsync(
        DateTimeOffset? activeSince = null,
        int limit = 100,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        int safeLimit = Math.Max(1, limit);

        using var activity = Activity.Current is not null ? Activity.Current.Source.StartActivity("db.session_thread.list_active") : null;
        activity?.SetTag("db.system", "efcore");
        activity?.SetTag("db.entity", "VKSessionThread");
        activity?.SetTag("db.operation", "ListActiveSessions");

        try
        {
            // [CS.04] Push pagination & predicate down to SQL; never scan full table
            var pagedResult = await _repository.GetPagedAsync(
                predicate: s => s.Status == VKSessionStatus.Active &&
                    (!activeSince.HasValue || (s.LastActivityAt ?? s.UpdatedAt ?? s.CreatedAt) >= activeSince.Value),
                orderBy: s => s.LastActivityAt ?? s.UpdatedAt ?? s.CreatedAt,
                pageNumber: 1,
                pageSize: safeLimit,
                ascending: false,
                cancellationToken: ct).ConfigureAwait(false);

            var domainList = pagedResult.Items.Select(e => e.ToDomain()).ToList().AsReadOnly();
            activity?.SetTag("db.result.count", domainList.Count);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return VKResult.Success<IReadOnlyList<VKSessionThread>>(domainList);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogListSessionEntitiesError(ex);
            return VKResult.Failure<IReadOnlyList<VKSessionThread>>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }
}

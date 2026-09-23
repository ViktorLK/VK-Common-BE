using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Session.Internal;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="IVKPsycheSessionRepository"/>.
/// Follows AP.01 (sealed class default) and CS.03.
/// </summary>
internal sealed class InMemorySessionRepository : VKInMemoryAggregateRepository<VKSessionThread, VKSessionId>, IVKPsycheSessionRepository
{
    public InMemorySessionRepository()
    {
    }

    protected override VKError GetNotFoundError(VKSessionId id) => VKSessionErrors.NotFound;

    protected override VKError GetAlreadyExistsError(VKSessionId id) => VKSessionErrors.AlreadyExists;

    public Task<VKResult<IReadOnlyList<VKSessionThread>>> ListActiveSessionsAsync(
        DateTimeOffset? activeSince = null,
        int limit = 100,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        int safeLimit = Math.Max(1, limit);
        var query = Store.Values.Where(s => s.Status == VKSessionStatus.Active);

        if (activeSince.HasValue)
        {
            query = query.Where(s => (s.LastActivityAt ?? s.UpdatedAt ?? s.CreatedAt) >= activeSince.Value);
        }

        IReadOnlyList<VKSessionThread> list = [.. query
            .OrderByDescending(s => s.LastActivityAt ?? s.UpdatedAt ?? s.CreatedAt)
            .Take(safeLimit)];

        return Task.FromResult(VKResult.Success(list));
    }
}

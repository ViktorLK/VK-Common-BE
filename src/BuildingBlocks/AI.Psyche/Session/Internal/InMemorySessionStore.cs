using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Session.Internal;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="IVKPsycheSessionRepository"/>.
/// Follows AP.01 (sealed class default) and CS.03.
/// </summary>
internal sealed class InMemorySessionStore : IVKPsycheSessionRepository
{
    private readonly ConcurrentDictionary<VKSessionId, VKSessionThread> _sessions = new();
    private readonly TimeProvider _timeProvider;

    public InMemorySessionStore(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public Task<VKResult<VKSessionThread>> FindByIdAsync(VKSessionId id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (id.IsEmpty)
        {
            return Task.FromResult(VKResult.Failure<VKSessionThread>(VKSessionErrors.NotFound));
        }

        if (_sessions.TryGetValue(id, out var session))
        {
            return Task.FromResult(VKResult.Success(session));
        }

        return Task.FromResult(VKResult.Failure<VKSessionThread>(VKSessionErrors.NotFound));
    }

    public Task<VKResult<IReadOnlyList<VKSessionThread>>> ListByIdsAsync(
        IReadOnlyList<VKSessionId> ids,
        CancellationToken ct = default)
    {
        VKGuard.NotNull(ids);
        ct.ThrowIfCancellationRequested();

        var list = new List<VKSessionThread>(ids.Count);
        foreach (var id in ids)
        {
            if (_sessions.TryGetValue(id, out var session))
            {
                list.Add(session);
            }
        }

        return Task.FromResult(VKResult.Success<IReadOnlyList<VKSessionThread>>(list));
    }

    public Task<bool> ExistsAsync(VKSessionId id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(!id.IsEmpty && _sessions.ContainsKey(id));
    }

    public Task<VKResult> AddAsync(VKSessionThread item, CancellationToken ct = default)
    {
        VKGuard.NotNull(item);
        ct.ThrowIfCancellationRequested();

        if (!_sessions.TryAdd(item.Id, item))
        {
            return Task.FromResult(VKResult.Failure(VKSessionErrors.NotFound));
        }

        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult> UpdateAsync(VKSessionThread item, CancellationToken ct = default)
    {
        VKGuard.NotNull(item);
        ct.ThrowIfCancellationRequested();

        if (!_sessions.ContainsKey(item.Id))
        {
            return Task.FromResult(VKResult.Failure(VKSessionErrors.NotFound));
        }

        _sessions[item.Id] = item;
        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult> DeleteAsync(VKSessionId id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (id.IsEmpty || !_sessions.TryRemove(id, out _))
        {
            return Task.FromResult(VKResult.Failure(VKSessionErrors.NotFound));
        }

        return Task.FromResult(VKResult.Success());
    }


    public InMemorySessionStore Seed(VKSessionThread session)
    {
        VKGuard.NotNull(session);
        _sessions[session.Id] = session;
        return this;
    }
}

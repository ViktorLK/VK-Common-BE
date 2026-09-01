using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Profile.Internal;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="IVKPsycheProfileRepository"/>.
/// Follows AP.01 (sealed class default) and CS.03.
/// </summary>
internal sealed class InMemoryProfileRepository : IVKPsycheProfileRepository
{
    private readonly ConcurrentDictionary<VKProfileId, VKProfilePresence> _presences = new();

    public InMemoryProfileRepository()
    {
    }

    public Task<VKResult<VKProfilePresence>> FindByIdAsync(VKProfileId id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (id.IsEmpty)
        {
            return Task.FromResult(VKResult.Failure<VKProfilePresence>(VKProfileErrors.NotFound));
        }

        if (_presences.TryGetValue(id, out var presence))
        {
            return Task.FromResult(VKResult.Success(presence));
        }

        return Task.FromResult(VKResult.Failure<VKProfilePresence>(VKProfileErrors.NotFound));
    }

    public Task<VKResult<IReadOnlyList<VKProfilePresence>>> ListByIdsAsync(
        IReadOnlyList<VKProfileId> ids,
        CancellationToken ct = default)
    {
        VKGuard.NotNull(ids);
        ct.ThrowIfCancellationRequested();

        var list = new List<VKProfilePresence>(ids.Count);
        foreach (var id in ids)
        {
            if (_presences.TryGetValue(id, out var presence))
            {
                list.Add(presence);
            }
        }

        return Task.FromResult(VKResult.Success<IReadOnlyList<VKProfilePresence>>(list));
    }

    public Task<VKResult<IReadOnlyList<VKProfilePresence>>> ListAllAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        IReadOnlyList<VKProfilePresence> list = [.. _presences.Values];
        return Task.FromResult(VKResult.Success(list));
    }

    public Task<bool> ExistsAsync(VKProfileId id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(!id.IsEmpty && _presences.ContainsKey(id));
    }

    public Task<VKResult> AddAsync(VKProfilePresence item, CancellationToken ct = default)
    {
        VKGuard.NotNull(item);
        ct.ThrowIfCancellationRequested();

        if (!_presences.TryAdd(item.Id, item))
        {
            return Task.FromResult(VKResult.Failure(VKProfileErrors.NotFound));
        }

        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult> UpdateAsync(VKProfilePresence item, CancellationToken ct = default)
    {
        VKGuard.NotNull(item);
        ct.ThrowIfCancellationRequested();

        if (!_presences.ContainsKey(item.Id))
        {
            return Task.FromResult(VKResult.Failure(VKProfileErrors.NotFound));
        }

        _presences[item.Id] = item;
        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult> DeleteAsync(VKProfileId id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (id.IsEmpty || !_presences.TryRemove(id, out _))
        {
            return Task.FromResult(VKResult.Failure(VKProfileErrors.NotFound));
        }

        return Task.FromResult(VKResult.Success());
    }


    public InMemoryProfileRepository Seed(VKProfilePresence presence)
    {
        VKGuard.NotNull(presence);
        _presences[presence.Id] = presence;
        return this;
    }

    public InMemoryProfileRepository Remove(VKProfileId profileId)
    {
        _presences.TryRemove(profileId, out _);
        return this;
    }

    public InMemoryProfileRepository Clear()
    {
        _presences.Clear();
        return this;
    }
}

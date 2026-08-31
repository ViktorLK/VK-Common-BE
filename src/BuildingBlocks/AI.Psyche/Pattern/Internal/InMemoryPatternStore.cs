using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Pattern.Internal;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="IVKPsychePatternRepository"/>.
/// Follows AP.01 (sealed class default) and CS.03.
/// </summary>
internal sealed class InMemoryPatternStore : IVKPsychePatternRepository
{
    private readonly ConcurrentDictionary<VKPatternId, VKPatternEntry> _patterns = new();

    public InMemoryPatternStore()
    {
    }

    public InMemoryPatternStore(IEnumerable<VKPatternEntry> patterns)
    {
        Seed(patterns);
    }

    public Task<VKResult<VKPatternEntry>> FindByIdAsync(VKPatternId id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (id.IsEmpty)
        {
            return Task.FromResult(VKResult.Failure<VKPatternEntry>(VKPatternErrors.NotFound));
        }

        if (_patterns.TryGetValue(id, out var pattern))
        {
            return Task.FromResult(VKResult.Success(pattern));
        }

        return Task.FromResult(VKResult.Failure<VKPatternEntry>(VKPatternErrors.NotFound));
    }

    public Task<VKResult<IReadOnlyList<VKPatternEntry>>> ListByIdsAsync(
        IReadOnlyList<VKPatternId> ids,
        CancellationToken ct = default)
    {
        VKGuard.NotNull(ids);
        ct.ThrowIfCancellationRequested();

        var list = new List<VKPatternEntry>(ids.Count);
        foreach (var id in ids)
        {
            if (_patterns.TryGetValue(id, out var pattern))
            {
                list.Add(pattern);
            }
        }

        return Task.FromResult(VKResult.Success<IReadOnlyList<VKPatternEntry>>(list));
    }

    public Task<VKResult<IReadOnlyList<VKPatternEntry>>> ListAllAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        IReadOnlyList<VKPatternEntry> list = [.. _patterns.Values];
        return Task.FromResult(VKResult.Success(list));
    }

    public Task<bool> ExistsAsync(VKPatternId id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(!id.IsEmpty && _patterns.ContainsKey(id));
    }

    public Task<VKResult> AddAsync(VKPatternEntry item, CancellationToken ct = default)
    {
        VKGuard.NotNull(item);
        ct.ThrowIfCancellationRequested();

        if (!_patterns.TryAdd(item.Id, item))
        {
            return Task.FromResult(VKResult.Failure(VKPatternErrors.AlreadyExists));
        }

        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult> UpdateAsync(VKPatternEntry item, CancellationToken ct = default)
    {
        VKGuard.NotNull(item);
        ct.ThrowIfCancellationRequested();

        if (!_patterns.ContainsKey(item.Id))
        {
            return Task.FromResult(VKResult.Failure(VKPatternErrors.NotFound));
        }

        _patterns[item.Id] = item;
        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult> DeleteAsync(VKPatternId id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (id.IsEmpty || !_patterns.TryRemove(id, out _))
        {
            return Task.FromResult(VKResult.Failure(VKPatternErrors.NotFound));
        }

        return Task.FromResult(VKResult.Success());
    }


    public InMemoryPatternStore Seed(VKPatternEntry pattern)
    {
        VKGuard.NotNull(pattern);
        _patterns[pattern.Id] = pattern;
        return this;
    }

    public InMemoryPatternStore Seed(IEnumerable<VKPatternEntry> patterns)
    {
        VKGuard.NotNull(patterns);
        foreach (var pattern in patterns)
        {
            _patterns[pattern.Id] = pattern;
        }
        return this;
    }

    public InMemoryPatternStore Remove(VKPatternId id)
    {
        _patterns.TryRemove(id, out _);
        return this;
    }

    public InMemoryPatternStore Clear()
    {
        _patterns.Clear();
        return this;
    }
}

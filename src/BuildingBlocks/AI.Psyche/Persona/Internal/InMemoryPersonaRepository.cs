using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Persona.Internal;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="IVKPsychePersonaRepository"/>.
/// Follows AP.01 (sealed class default) and CS.03.
/// </summary>
internal sealed class InMemoryPersonaRepository : IVKPsychePersonaRepository
{
    private readonly ConcurrentDictionary<VKPersonaId, VKPersonaAnchor> _store = new();

    public InMemoryPersonaRepository()
    {
    }

    public Task<VKResult<VKPersonaAnchor>> FindByIdAsync(VKPersonaId id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (id.IsEmpty)
        {
            return Task.FromResult(VKResult.Failure<VKPersonaAnchor>(VKPersonaErrors.NotFound));
        }

        if (_store.TryGetValue(id, out var anchor))
        {
            return Task.FromResult(VKResult.Success(anchor));
        }

        return Task.FromResult(VKResult.Failure<VKPersonaAnchor>(VKPersonaErrors.NotFound));
    }

    public Task<VKResult<IReadOnlyList<VKPersonaAnchor>>> ListByIdsAsync(
        IReadOnlyList<VKPersonaId> ids,
        CancellationToken ct = default)
    {
        VKGuard.NotNull(ids);
        ct.ThrowIfCancellationRequested();

        var list = new List<VKPersonaAnchor>(ids.Count);
        foreach (var id in ids)
        {
            if (_store.TryGetValue(id, out var anchor))
            {
                list.Add(anchor);
            }
        }

        return Task.FromResult(VKResult.Success<IReadOnlyList<VKPersonaAnchor>>(list));
    }

    public Task<VKResult<IReadOnlyList<VKPersonaAnchor>>> ListAllAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        IReadOnlyList<VKPersonaAnchor> list = [.. _store.Values];
        return Task.FromResult(VKResult.Success(list));
    }

    public Task<bool> ExistsAsync(VKPersonaId id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(!id.IsEmpty && _store.ContainsKey(id));
    }

    public Task<VKResult> AddAsync(VKPersonaAnchor item, CancellationToken ct = default)
    {
        VKGuard.NotNull(item);
        ct.ThrowIfCancellationRequested();

        if (!_store.TryAdd(item.Id, item))
        {
            return Task.FromResult(VKResult.Failure(VKPersonaErrors.AlreadyExists));
        }

        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult> UpdateAsync(VKPersonaAnchor item, CancellationToken ct = default)
    {
        VKGuard.NotNull(item);
        ct.ThrowIfCancellationRequested();

        if (!_store.ContainsKey(item.Id))
        {
            return Task.FromResult(VKResult.Failure(VKPersonaErrors.NotFound));
        }

        _store[item.Id] = item;
        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult> DeleteAsync(VKPersonaId id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (id.IsEmpty || !_store.TryRemove(id, out _))
        {
            return Task.FromResult(VKResult.Failure(VKPersonaErrors.NotFound));
        }

        return Task.FromResult(VKResult.Success());
    }


    public InMemoryPersonaRepository Seed(VKPersonaAnchor persona)
    {
        VKGuard.NotNull(persona);
        _store[persona.Id] = persona;
        return this;
    }

    public InMemoryPersonaRepository Seed(IEnumerable<VKPersonaAnchor> personas)
    {
        VKGuard.NotNull(personas);
        foreach (var p in personas)
        {
            _store[p.Id] = p;
        }
        return this;
    }

    public InMemoryPersonaRepository Remove(VKPersonaId personaId)
    {
        _store.TryRemove(personaId, out _);
        return this;
    }

    public InMemoryPersonaRepository Clear()
    {
        _store.Clear();
        return this;
    }
}

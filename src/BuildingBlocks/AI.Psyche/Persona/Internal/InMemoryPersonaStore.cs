using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Persona.Internal;

/// <summary>
/// Basic concrete implementation of <see cref="IVKPersonaStore"/>.
/// Follows AP.01 and CS.03.
/// </summary>
internal sealed class InMemoryPersonaStore : IVKPersonaStore
{
    private readonly ConcurrentDictionary<VKPersonaId, VKPersonaAnchor> _store = new();

    public InMemoryPersonaStore()
    {
    }

    public Task<VKResult<IReadOnlyList<VKPersonaAnchor>>> GetPersonasAsync(
        IReadOnlyList<VKPersonaId> personaIds,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(personaIds);
        cancellationToken.ThrowIfCancellationRequested();

        var list = new List<VKPersonaAnchor>(personaIds.Count);
        foreach (var personaId in personaIds)
        {
            if (_store.TryGetValue(personaId, out var anchor))
            {
                list.Add(anchor);
            }
        }

        return Task.FromResult(VKResult.Success<IReadOnlyList<VKPersonaAnchor>>(list));
    }

    public InMemoryPersonaStore Seed(VKPersonaAnchor persona)
    {
        VKGuard.NotNull(persona);

        _store[persona.Id] = persona;

        return this;
    }

    public InMemoryPersonaStore Seed(IEnumerable<VKPersonaAnchor> personas)
    {
        VKGuard.NotNull(personas);

        foreach (var p in personas)
        {
            _store[p.Id] = p;
        }

        return this;
    }

    public InMemoryPersonaStore Remove(VKPersonaId personaId)
    {
        VKGuard.NotEmptyGuid(personaId.Value);

        _store.TryRemove(personaId, out _);

        return this;
    }

    public InMemoryPersonaStore Clear()
    {
        _store.Clear();

        return this;
    }
}

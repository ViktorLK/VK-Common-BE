using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Persona.Internal;

/// <summary>
/// Basic concrete implementation of <see cref="IVKPersonaStore"/>.
/// Offers thread-safe in-memory backing storage for persona cards.
/// </summary>
internal sealed class InMemoryPersonaStore : IVKPersonaStore
{
    private readonly ConcurrentDictionary<VKPersonaId, VKPersonaAnchor> _store = new();

    public Task<VKResult<VKPersonaAnchor>> GetPersonaAsync(
        VKPersonaId personaId,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotEmptyGuid(personaId.Value);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_store.TryGetValue(personaId, out var anchor))
        {
            return Task.FromResult(VKResult.Failure<VKPersonaAnchor>(VKPersonaErrors.NotFound));
        }

        return Task.FromResult(VKResult.Success(anchor));
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

using System;
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
    private readonly ConcurrentDictionary<string, VKPersonaAnchor> _store = new(StringComparer.OrdinalIgnoreCase);

    public Task<VKResult<VKPersonaAnchor>> GetPersonaAsync(
        string personaId,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(personaId);
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

    public InMemoryPersonaStore Remove(string personaId)
    {
        VKGuard.NotNullOrWhiteSpace(personaId);

        _store.TryRemove(personaId, out _);

        return this;
    }

    public InMemoryPersonaStore Clear()
    {
        _store.Clear();

        return this;
    }
}

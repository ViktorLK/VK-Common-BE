using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Persona.Internal;

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
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNullOrWhiteSpace(personaId);

        if (!_store.TryGetValue(personaId, out var anchor))
        {
            return Task.FromResult(VKResult.Failure<VKPersonaAnchor>(VKKnowledgeErrors.NotFound));
        }

        return Task.FromResult(VKResult.Success(anchor));
    }

    public InMemoryPersonaStore Seed(VKPersonaAnchor persona)
    {
        _store[persona.Id] = persona;

        return this;
    }

    public InMemoryPersonaStore Seed(IEnumerable<VKPersonaAnchor> personas)
    {
        foreach (var p in personas)
        {
            _store[p.Id] = p;
        }

        return this;
    }

    public InMemoryPersonaStore Remove(string personaId)
    {
        _store.TryRemove(personaId, out _);

        return this;
    }

    public InMemoryPersonaStore Clear()
    {
        _store.Clear();

        return this;
    }
}

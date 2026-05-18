using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Persona.Internal;

/// <summary>
/// Basic concrete implementation of <see cref="IVKPersonaCodex"/>.
/// Offers thread-safe in-memory backing storage for persona cards.
/// </summary>
internal sealed class BasicPersonaCodex : IVKPersonaCodex
{
    private readonly ConcurrentDictionary<string, VKPersonaAnchor> _store = new(StringComparer.OrdinalIgnoreCase);

    public Task<VKResult> AddPersonaAsync(
        VKPersonaAnchor anchor,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // [AP.01]
        VKGuard.NotNull(anchor);
        VKGuard.NotNullOrWhiteSpace(anchor.Id);

        // [CS.01]
        if (!_store.TryAdd(anchor.Id, anchor))
        {
            return Task.FromResult(VKResult.Failure(VKPersonaErrors.AlreadyExists));
        }

        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult<VKPersonaAnchor>> GetPersonaAsync(
        string personaId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNullOrWhiteSpace(personaId);

        if (!_store.TryGetValue(personaId, out var anchor))
        {
            return Task.FromResult(VKResult.Failure<VKPersonaAnchor>(VKPersonaErrors.NotFound));
        }

        return Task.FromResult(VKResult.Success(anchor));
    }

    public Task<VKResult<IEnumerable<VKPersonaAnchor>>> GetAllPersonasAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var list = _store.Values.ToList();
        return Task.FromResult(VKResult.Success<IEnumerable<VKPersonaAnchor>>(list));
    }

    public Task<VKResult> UpdatePersonaAsync(
        string personaId,
        VKPersonaAnchor updatedAnchor,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNullOrWhiteSpace(personaId);
        VKGuard.NotNull(updatedAnchor);

        _store[personaId] = updatedAnchor;
        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult> DeletePersonaAsync(
        string personaId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNullOrWhiteSpace(personaId);

        _store.TryRemove(personaId, out _);
        return Task.FromResult(VKResult.Success());
    }
}

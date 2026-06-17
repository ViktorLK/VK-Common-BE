using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Echo.Internal;

/// <summary>
/// Basic concrete implementation of <see cref="IVKEchoStore"/>.
/// Provides a high-performance in-memory backing store for short-term conversation history.
/// Follows AP.01 and CS.03.
/// </summary>
internal sealed class InMemoryEchoStore : IVKEchoStore
{
    private readonly ConcurrentDictionary<VKSessionId, List<VKEchoTrace>> _store = new();

    public Task<VKResult<IReadOnlyCollection<VKEchoTrace>>> GetHistoryAsync(
        VKSessionId sessionId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotEmptyGuid(sessionId.Value);

        if (!_store.TryGetValue(sessionId, out var traces))
        {
            return Task.FromResult(VKResult.Success<IReadOnlyCollection<VKEchoTrace>>([]));
        }

        lock (traces)
        {
            IReadOnlyCollection<VKEchoTrace> copy = [.. traces];
            return Task.FromResult(VKResult.Success(copy));
        }
    }

    public InMemoryEchoStore Seed(VKSessionId sessionId, VKEchoTrace trace)
    {
        VKGuard.NotEmptyGuid(sessionId.Value);
        VKGuard.NotNull(trace);

        var list = _store.GetOrAdd(sessionId, _ => []);
        lock (list)
        {
            list.Add(trace);
        }

        return this;
    }

    public InMemoryEchoStore Seed(VKSessionId sessionId, IEnumerable<VKEchoTrace> echoes)
    {
        VKGuard.NotEmptyGuid(sessionId.Value);
        VKGuard.NotNull(echoes);

        var list = _store.GetOrAdd(sessionId, _ => []);
        lock (list)
        {
            list.AddRange(echoes);
        }

        return this;
    }

    public InMemoryEchoStore Remove(VKSessionId sessionId)
    {
        VKGuard.NotEmptyGuid(sessionId.Value);
        _store.TryRemove(sessionId, out _);

        return this;
    }

    public InMemoryEchoStore Clear()
    {
        _store.Clear();

        return this;
    }
}

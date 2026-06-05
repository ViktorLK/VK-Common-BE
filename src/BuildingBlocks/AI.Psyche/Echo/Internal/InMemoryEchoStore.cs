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
        if (sessionId.IsEmpty) throw new ArgumentException("SessionId cannot be empty.", nameof(sessionId));

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
        if (sessionId.IsEmpty) throw new ArgumentException("SessionId cannot be empty.", nameof(sessionId));
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
        if (sessionId.IsEmpty) throw new ArgumentException("SessionId cannot be empty.", nameof(sessionId));
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
        if (sessionId.IsEmpty) throw new ArgumentException("SessionId cannot be empty.", nameof(sessionId));
        _store.TryRemove(sessionId, out _);

        return this;
    }

    public InMemoryEchoStore Clear()
    {
        _store.Clear();

        return this;
    }
}

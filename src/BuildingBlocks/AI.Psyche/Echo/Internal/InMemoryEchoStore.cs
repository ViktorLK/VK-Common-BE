using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Echo.Internal;

/// <summary>
/// Basic concrete implementation of <see cref="IVKEchoStore"/>.
/// Provides a high-performance in-memory backing store for short-term conversation history.
/// Supports 2-phase retrieval: lightweight metadata projection and ID-based batch fetch.
/// Follows AP.01 and CS.03.
/// </summary>
internal sealed class InMemoryEchoStore : IVKEchoStore
{
    private readonly ConcurrentDictionary<VKSessionId, List<VKEchoTrace>> _store = new();

    public InMemoryEchoStore()
    {
    }

    public Task<VKResult<IReadOnlyCollection<VKEchoMetadata>>> GetMetadataAsync(
        VKSessionId sessionId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotEmptyGuid(sessionId.Value);

        if (!_store.TryGetValue(sessionId, out var traces))
        {
            return Task.FromResult(VKResult.Success<IReadOnlyCollection<VKEchoMetadata>>([]));
        }

        lock (traces)
        {
            var metaList = traces.Select(t => new VKEchoMetadata
            {
                Id = t.Id,
                SessionId = t.SessionId,
                Role = t.Role,
                TokenCount = t.TokenCount,
                CreatedAt = t.CreatedAt
            }).ToList();

            IReadOnlyCollection<VKEchoMetadata> copy = metaList;
            return Task.FromResult(VKResult.Success(copy));
        }
    }

    public Task<VKResult<IReadOnlyCollection<VKEchoTrace>>> GetTracesByIdsAsync(
        IReadOnlyCollection<VKEchoId> ids,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNull(ids);

        if (ids.Count == 0)
        {
            return Task.FromResult(VKResult.Success<IReadOnlyCollection<VKEchoTrace>>([]));
        }

        var idSet = new HashSet<VKEchoId>(ids);
        var result = new List<VKEchoTrace>();

        foreach (var pair in _store)
        {
            lock (pair.Value)
            {
                result.AddRange(pair.Value.Where(t => idSet.Contains(t.Id)));
            }
        }

        var ordered = result.OrderBy(t => t.CreatedAt).ToList();
        IReadOnlyCollection<VKEchoTrace> copy = ordered;
        return Task.FromResult(VKResult.Success(copy));
    }

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
            var ordered = traces.OrderBy(t => t.CreatedAt).ToList();
            IReadOnlyCollection<VKEchoTrace> copy = ordered;
            return Task.FromResult(VKResult.Success(copy));
        }
    }

    public Task<VKResult> SaveHistoryAsync(
        VKEchoTrace trace,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNull(trace);

        Seed(trace.SessionId, trace);
        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult> SaveHistoryBatchAsync(
        IReadOnlyCollection<VKEchoTrace> traces,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNull(traces);

        foreach (var trace in traces)
        {
            Seed(trace.SessionId, trace);
        }

        return Task.FromResult(VKResult.Success());
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

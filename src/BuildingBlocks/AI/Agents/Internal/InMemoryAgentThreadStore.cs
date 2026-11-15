using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Agents.Internal;

/// <summary>
/// In-memory fallback implementation of <see cref="IVKAgentThreadStore"/>.
/// Adheres to ARCH-05 (Co-located fallback implementations) and CS.01.
/// </summary>
internal sealed class InMemoryAgentThreadStore : IVKAgentThreadStore
{
    private readonly ConcurrentDictionary<string, Dictionary<string, object>> _store = new(StringComparer.OrdinalIgnoreCase);

    public Task<VKResult<IReadOnlyDictionary<string, object>?>> GetThreadStateAsync(
        string threadId,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(threadId);
        cancellationToken.ThrowIfCancellationRequested();

        if (_store.TryGetValue(threadId, out var state))
        {
            var copy = new Dictionary<string, object>(state);
            return Task.FromResult(VKResult.Success<IReadOnlyDictionary<string, object>?>(copy));
        }

        return Task.FromResult(VKResult.Success<IReadOnlyDictionary<string, object>?>(null));
    }

    public Task<VKResult> SaveThreadStateAsync(
        string threadId,
        IReadOnlyDictionary<string, object> state,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(threadId);
        VKGuard.NotNull(state);
        cancellationToken.ThrowIfCancellationRequested();

        _store[threadId] = new Dictionary<string, object>(state);
        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult> DeleteThreadStateAsync(
        string threadId,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(threadId);
        cancellationToken.ThrowIfCancellationRequested();

        _store.TryRemove(threadId, out _);
        return Task.FromResult(VKResult.Success());
    }
}

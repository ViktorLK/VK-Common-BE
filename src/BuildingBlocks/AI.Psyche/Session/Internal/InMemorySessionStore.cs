using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Session.Internal;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="IVKSessionStore"/> for local development and testing.
/// Follows AP.01 (sealed class default) and CS.03.
/// </summary>
internal sealed class InMemorySessionStore : IVKSessionStore
{
    private readonly ConcurrentDictionary<VKSessionId, VKSessionThread> _sessions = new();
    private readonly TimeProvider _timeProvider;

    public InMemorySessionStore(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public Task<VKResult<VKSessionThread?>> GetSessionAsync(
        VKSessionId sessionId,
        CancellationToken cancellationToken = default)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            return Task.FromResult(VKResult.Success<VKSessionThread?>(null));
        }

        return Task.FromResult(VKResult.Success<VKSessionThread?>(session));
    }

    public Task<VKResult> UpdateSessionAsync(
        VKSessionThread session,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(session);
        _sessions[session.Id] = session;
        return Task.FromResult(VKResult.Success());
    }

    /// <summary>
    /// Seeds a session thread entry into the in-memory store for local testing.
    /// </summary>
    public InMemorySessionStore Seed(VKSessionThread session)
    {
        VKGuard.NotNull(session);
        _sessions[session.Id] = session;
        return this;
    }
}

using System;
using System.Collections.Concurrent;
using System.Linq;
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

    public Task<VKResult<VKSessionThread?>> GetSessionAsync(
        VKSessionId sessionId,
        CancellationToken cancellationToken = default)
    {
        _sessions.TryGetValue(sessionId, out var session);
        return Task.FromResult(VKResult.Success<VKSessionThread?>(session));
    }

    public Task<VKResult<VKSessionThread?>> GetActiveSessionAsync(
        VKPersonaId personaId,
        VKUserId? userId = null,
        CancellationToken cancellationToken = default)
    {
        var targetUser = userId ?? VKUserId.Anonymous;
        var activeSession = _sessions.Values
            .FirstOrDefault(s => s.PersonaId == personaId && s.UserId == targetUser && s.Status == VKSessionStatus.Active);

        return Task.FromResult(VKResult.Success<VKSessionThread?>(activeSession));
    }

    public Task<VKResult> SaveSessionAsync(
        VKSessionThread session,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(session);
        _sessions[session.Id] = session;
        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult> TouchSessionAsync(
        VKSessionId sessionId,
        CancellationToken cancellationToken = default)
    {
        if (_sessions.TryGetValue(sessionId, out var existing))
        {
            var updated = existing with
            {
                TurnCount = existing.TurnCount + 1,
                LastActivityAt = TimeProvider.System.GetUtcNow(),
                UpdatedAt = TimeProvider.System.GetUtcNow()
            };
            _sessions[sessionId] = updated;
        }

        return Task.FromResult(VKResult.Success());
    }
}

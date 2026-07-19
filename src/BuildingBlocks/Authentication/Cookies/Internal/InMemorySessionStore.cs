using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Authentication.Cookies.Protocols;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.Cookies.Internal;

/// <summary>
/// A thread-safe, in-memory implementation of <see cref="IVKSessionStore"/>.
/// </summary>
internal sealed class InMemorySessionStore : IVKSessionStore
{
    private readonly ConcurrentDictionary<string, VKSessionInfo> _sessions = new();
    private readonly ConcurrentDictionary<string, ConcurrentQueue<string>> _userSessions = new();

    /// <inheritdoc />
    public ValueTask<VKResult<string>> RegisterSessionAsync(
        string userId,
        string ticketData,
        DateTimeOffset expiresAt,
        int maxConcurrentSessions,
        bool kickOldest,
        CancellationToken ct = default)
    {
        string sessionId = Guid.NewGuid().ToString("N");
        var session = new VKSessionInfo
        {
            SessionId = sessionId,
            UserId = userId,
            TicketData = ticketData,
            ExpiresAt = expiresAt,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _sessions.TryAdd(sessionId, session);

        var queue = _userSessions.GetOrAdd(userId, _ => new ConcurrentQueue<string>());
        lock (queue)
        {
            queue.Enqueue(sessionId);

            if (maxConcurrentSessions > 0 && queue.Count > maxConcurrentSessions)
            {
                if (kickOldest)
                {
                    while (queue.Count > maxConcurrentSessions)
                    {
                        if (queue.TryDequeue(out string? oldestSessionId))
                        {
                            _sessions.TryRemove(oldestSessionId, out _);
                        }
                    }
                }
                else
                {
                    _sessions.TryRemove(sessionId, out _);
                    
                    // Filter out the sessionId we just rejected
                    var activeList = queue.Where(x => x != sessionId).ToList();
                    queue.Clear();
                    foreach (var id in activeList)
                    {
                        queue.Enqueue(id);
                    }

                    return ValueTask.FromResult(VKResult.Failure<string>(new VKError(
                        "Auth.SessionLimitExceeded",
                        "Maximum concurrent sessions exceeded. Log-in rejected.",
                        VKErrorType.Forbidden)));
                }
            }
        }

        return ValueTask.FromResult(VKResult.Success(sessionId));
    }

    /// <inheritdoc />
    public ValueTask<string?> GetSessionTicketAsync(string sessionId, CancellationToken ct = default)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            if (session.ExpiresAt > DateTimeOffset.UtcNow)
            {
                return ValueTask.FromResult<string?>(session.TicketData);
            }
            _sessions.TryRemove(sessionId, out _);
        }
        return ValueTask.FromResult<string?>(null);
    }

    /// <inheritdoc />
    public ValueTask UpdateSessionTicketAsync(string sessionId, string ticketData, DateTimeOffset expiresAt, CancellationToken ct = default)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            session.TicketData = ticketData;
            session.ExpiresAt = expiresAt;
        }
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask RevokeSessionAsync(string sessionId, CancellationToken ct = default)
    {
        _sessions.TryRemove(sessionId, out _);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask<bool> IsSessionActiveAsync(string sessionId, CancellationToken ct = default)
    {
        return ValueTask.FromResult(_sessions.TryGetValue(sessionId, out var session) && session.ExpiresAt > DateTimeOffset.UtcNow);
    }

    /// <inheritdoc />
    public ValueTask RevokeUserSessionsAsync(string userId, CancellationToken ct = default)
    {
        VKGuard.NotNullOrWhiteSpace(userId);

        if (_userSessions.TryRemove(userId, out var queue))
        {
            while (queue.TryDequeue(out string? sessionId))
            {
                _sessions.TryRemove(sessionId, out _);
            }
        }

        return ValueTask.CompletedTask;
    }
}

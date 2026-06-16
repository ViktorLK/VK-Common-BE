using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Compression.Internal;

/// <summary>
/// In-memory implementation of <see cref="IVKChatSessionStore"/>.
/// Follows AP.01 (sealed class) and CS.03.
/// </summary>
internal sealed class InMemoryChatSessionStore : IVKChatSessionStore
{
    private readonly ConcurrentDictionary<VKChatSessionId, VKChatSession> _sessions = new();
    private readonly TimeProvider _timeProvider;

    public InMemoryChatSessionStore(TimeProvider timeProvider)
    {
        _timeProvider = VKGuard.NotNull(timeProvider);
    }

    public Task<VKResult<VKChatSession>> GetAsync(
        VKChatSessionId id,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (id.IsEmpty) throw new ArgumentException("SessionId cannot be empty.", nameof(id));

        if (_sessions.TryGetValue(id, out var session))
        {
            return Task.FromResult(VKResult.Success(session));
        }

        return Task.FromResult(VKResult.Failure<VKChatSession>(Errors.SessionNotFound));
    }

    public Task<VKResult> UpdateSummaryAsync(
        VKChatSessionId id,
        string summary,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (id.IsEmpty) throw new ArgumentException("SessionId cannot be empty.", nameof(id));
        VKGuard.NotNull(summary);

        var now = _timeProvider.GetUtcNow();

        _sessions.AddOrUpdate(
            id,
            addValueFactory: _ => new VKChatSession
            {
                Id = id,
                PersonaId = VKPersonaId.Empty,
                Summary = summary,
                CreatedAt = now,
                UpdatedAt = now
            },
            updateValueFactory: (_, existing) => existing with
            {
                Summary = summary,
                UpdatedAt = now
            });

        return Task.FromResult(VKResult.Success());
    }

    /// <summary>
    /// Seeds the store with a session for testing or initial state.
    /// </summary>
    public void Seed(VKChatSession session)
    {
        VKGuard.NotNull(session);
        _sessions[session.Id] = session;
    }
}

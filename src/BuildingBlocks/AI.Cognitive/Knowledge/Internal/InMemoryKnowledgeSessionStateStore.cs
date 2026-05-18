using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Knowledge.Internal;

/// <summary>
/// Thread-safe in-memory backing implementation of <see cref="IVKKnowledgeSessionStateStore"/>.
/// </summary>
internal sealed class InMemoryKnowledgeSessionStateStore : IVKKnowledgeSessionStateStore
{
    private readonly ConcurrentDictionary<(string SessionId, string KnowledgeId), VKKnowledgeSessionState> _store = new();

    public Task<VKResult<VKKnowledgeSessionState?>> GetStateAsync(
        string sessionId,
        string knowledgeId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNullOrWhiteSpace(sessionId);
        VKGuard.NotNullOrWhiteSpace(knowledgeId);

        _store.TryGetValue((sessionId, knowledgeId), out var state);
        return Task.FromResult(VKResult.Success<VKKnowledgeSessionState?>(state));
    }

    public Task<VKResult> SaveStateAsync(
        VKKnowledgeSessionState state,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNull(state);

        _store[(state.SessionId, state.KnowledgeId)] = state;
        return Task.FromResult(VKResult.Success());
    }
}

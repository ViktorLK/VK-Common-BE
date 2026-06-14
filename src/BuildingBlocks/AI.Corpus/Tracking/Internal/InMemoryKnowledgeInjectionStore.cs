using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// An in-memory, basic implementation of the <see cref="IVKKnowledgeInjectionStore"/>.
/// Follows AP.01 and the "Basic" taxonomy of AP.03.
/// </summary>
internal sealed class InMemoryKnowledgeInjectionStore : IVKKnowledgeInjectionStore
{
    private readonly ConcurrentDictionary<VKSessionId, List<VKKnowledgeInjection>> _records = new();

    /// <inheritdoc />
    public Task<VKResult> RecordInjectionsAsync(
        VKSessionId sessionId,
        IReadOnlyCollection<VKKnowledgeInjection> injections,
        CancellationToken cancellationToken = default)
    {
        if (sessionId.IsEmpty)
        {
            throw new ArgumentException("SessionId cannot be empty.", nameof(sessionId));
        }
        VKGuard.NotNull(injections);

        List<VKKnowledgeInjection> list = _records.GetOrAdd(sessionId, _ => []);
        lock (list)
        {
            list.AddRange(injections);
        }

        return Task.FromResult(VKResult.Success());
    }

    /// <inheritdoc />
    public Task<VKResult<IReadOnlyCollection<VKKnowledgeInjection>>> GetInjectionsAsync(
        VKSessionId sessionId,
        CancellationToken cancellationToken = default)
    {
        if (sessionId.IsEmpty)
        {
            throw new ArgumentException("SessionId cannot be empty.", nameof(sessionId));
        }

        IReadOnlyCollection<VKKnowledgeInjection> result = Array.Empty<VKKnowledgeInjection>();

        if (_records.TryGetValue(sessionId, out List<VKKnowledgeInjection>? list))
        {
            lock (list)
            {
                result = [.. list];
            }
        }

        return Task.FromResult(VKResult.Success(result));
    }
}

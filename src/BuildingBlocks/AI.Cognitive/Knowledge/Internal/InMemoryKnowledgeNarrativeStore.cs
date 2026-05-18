using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Knowledge.Internal;

/// <summary>
/// Thread-safe in-memory backing implementation of <see cref="IVKKnowledgeNarrativeStore"/>.
/// </summary>
internal sealed class InMemoryKnowledgeNarrativeStore : IVKKnowledgeNarrativeStore
{
    private readonly ConcurrentDictionary<string, VKKnowledgeNarrativeRules> _store = new();

    public Task<VKResult<VKKnowledgeNarrativeRules?>> GetRulesAsync(
        string knowledgeId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNullOrWhiteSpace(knowledgeId);

        _store.TryGetValue(knowledgeId, out var rules);
        return Task.FromResult(VKResult.Success<VKKnowledgeNarrativeRules?>(rules));
    }

    public Task<VKResult> SaveRulesAsync(
        VKKnowledgeNarrativeRules rules,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNull(rules);

        _store[rules.KnowledgeId] = rules;
        return Task.FromResult(VKResult.Success());
    }
}

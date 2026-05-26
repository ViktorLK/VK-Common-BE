using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Knowledge.Internal;

// [AP.01] sealed default implementation
internal sealed class DefaultKnowledgePromptExtractor : IVKPromptExtractor
{
    private readonly IVKKnowledgeStore _store;

    public DefaultKnowledgePromptExtractor(IVKKnowledgeStore store)
    {
        // [AP.01] Defensive boundary check
        _store = VKGuard.NotNull(store);
    }

    public async Task<VKResult<IReadOnlyList<VKPromptFragment>>> ExtractAsync(
        VKOrchestrationPipelineContext context,
        CancellationToken ct)
    {
        // [AP.01] VKGuard boundary check
        VKGuard.NotNull(context);

        // 1. Fetch relevant entries from the store using context.PersonaId
        var knowledgeResult = await _store.GetRelevantEntriesAsync(context.PersonaId, ct).ConfigureAwait(false); // [CS.03]
        if (knowledgeResult.IsFailure)
        {
            return VKResult.Failure<IReadOnlyList<VKPromptFragment>>(knowledgeResult.FirstError); // [CS.01]
        }

        // 2. Wrap each entry into a fragment with Metadata set to the raw KnowledgeEntry
        var fragments = new List<VKPromptFragment>();
        foreach (var entry in knowledgeResult.Value)
        {
            fragments.Add(new VKPromptFragment
            {
                TierType = VKPromptTierType.Knowledge,
                Content = string.Empty, // Populated by IVKPromptFormatter downstream
                Position = VKKnowledgePositions.AfterDefs,
                Priority = 0,
                Metadata = entry // Pass the raw entity to formatting phase
            });
        }

        IReadOnlyList<VKPromptFragment> resultList = fragments; // [AP.01]
        return VKResult.Success(resultList);
    }
}

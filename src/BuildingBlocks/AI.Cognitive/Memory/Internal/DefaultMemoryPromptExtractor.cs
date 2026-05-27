using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Memory.Internal;

// [AP.01] sealed default implementation
internal sealed class DefaultMemoryPromptExtractor : IVKPromptExtractor
{
    private readonly IVKMemoryEchoes _echoes;

    public DefaultMemoryPromptExtractor(IVKMemoryEchoes echoes)
    {
        // [AP.01] Defensive boundary check
        _echoes = VKGuard.NotNull(echoes);
    }

    public async Task<VKResult<IReadOnlyList<VKPromptFragment>>> ExtractAsync(
        VKOrchestrationPipelineContext context,
        CancellationToken ct)
    {
        // [AP.01] VKGuard boundary check
        VKGuard.NotNull(context);

        if (string.IsNullOrWhiteSpace(context.Input))
        {
            IReadOnlyList<VKPromptFragment> empty = [];
            return VKResult.Success(empty);
        }

        // 1. Fetch relevant memories from the store using context.Input
        var searchResult = await _echoes.SearchAsync(context.Input, limit: 5, minScore: 0.5f, ct).ConfigureAwait(false); // [CS.03]
        if (searchResult.IsFailure)
        {
            return VKResult.Failure<IReadOnlyList<VKPromptFragment>>(searchResult.FirstError); // [CS.01]
        }

        // 2. Wrap each memory into a fragment
        var fragments = new List<VKPromptFragment>();
        foreach (var queryResult in searchResult.Value)
        {
            var entry = queryResult.Entry;
            fragments.Add(new VKPromptFragment
            {
                TierType = VKPromptTierType.Knowledge, // Treat memory as knowledge context
                Content = string.Empty, // Populated by IVKPromptFormatter downstream if using metadata
                Position = VKKnowledgePositions.AfterDefs,
                Priority = 0,
                Metadata = entry // Pass the raw entity
            });
        }

        IReadOnlyList<VKPromptFragment> resultList = fragments; // [AP.01]
        return VKResult.Success(resultList);
    }
}

using System.Collections.Generic;
using System.Linq;
using VK.Blocks.Core;

// // [AP.03] Internal implementation inside Internal/ folder without VK prefix
namespace VK.Blocks.AI.Cognitive.Weaving.Internal;

internal sealed class BasicPromptPruner : IVKPromptPruner
{
    public VKResult<IReadOnlyList<VKScoredFragment>> Prune(IReadOnlyList<VKScoredFragment> scored, VKOrchestrationPipelineContext context)
    {
        VKGuard.NotNull(scored);
        VKGuard.NotNull(context);

        var pruned = new List<VKScoredFragment>();
        var grouped = scored.GroupBy(s => s.Fragment.InclusionGroup);

        foreach (var group in grouped)
        {
            if (string.IsNullOrEmpty(group.Key))
            {
                pruned.AddRange(group);
            }
            else
            {
                // Pruning logic: take the highest scored fragment from the mutually exclusive group
                var survivor = group.OrderByDescending(s => s.Score).FirstOrDefault();
                if (survivor != null)
                {
                    pruned.Add(survivor);
                }
            }
        }

        return VKResult.Success<IReadOnlyList<VKScoredFragment>>(pruned);
    }
}

using System.Collections.Generic;
using VK.Blocks.Core;

// // [AP.03] Internal implementation inside Internal/ folder without VK prefix
namespace VK.Blocks.AI.Cognitive.Weaving.Internal;

internal sealed class BasicPromptScorer : IVKPromptScorer
{
    public VKResult<IReadOnlyList<VKScoredFragment>> Score(IReadOnlyList<VKPromptFragment> fragments, VKWeavingContext context)
    {
        VKGuard.NotNull(fragments);
        VKGuard.NotNull(context);

        var scored = new List<VKScoredFragment>();
        foreach (var fragment in fragments)
        {
            // Base scoring strategy. Extensible down the line.
            double score = fragment.GroupWeight + (100 - fragment.Priority);
            scored.Add(new VKScoredFragment
            {
                Fragment = fragment,
                Score = score,
                IsSticky = false
            });
        }
        return VKResult.Success<IReadOnlyList<VKScoredFragment>>(scored);
    }
}

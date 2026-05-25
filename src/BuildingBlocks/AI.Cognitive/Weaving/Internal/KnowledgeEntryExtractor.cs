using System.Collections.Generic;
using VK.Blocks.Core;

// // [AP.03] Internal implementation inside Internal/ folder without VK prefix
namespace VK.Blocks.AI.Cognitive.Weaving.Internal;

internal sealed class KnowledgeEntryExtractor : IVKPromptExtractor<IEnumerable<VKKnowledgeEntry>>
{
    public VKResult<IReadOnlyList<VKPromptFragment>> Extract(IEnumerable<VKKnowledgeEntry> source, VKWeavingContext context)
    {
        VKGuard.NotNull(source);
        VKGuard.NotNull(context);

        var fragments = new List<VKPromptFragment>();

        foreach (var entry in source)
        {
            fragments.Add(new VKPromptFragment
            {
                Id = entry.Id,
                Content = entry.Content,
                Position = entry.Weaving.Position,
                TierType = VKPromptTierType.Knowledge,
                Priority = entry.Weaving.Priority,
                Depth = entry.Weaving.Depth,
                GroupWeight = 100 // Default for standard entries, narrative rules handle specifics if needed
            });
        }

        return VKResult.Success<IReadOnlyList<VKPromptFragment>>(fragments);
    }
}

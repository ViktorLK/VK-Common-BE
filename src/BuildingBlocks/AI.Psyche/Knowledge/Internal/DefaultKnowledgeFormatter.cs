using System;
using System.Collections.Generic;
using System.Linq;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Knowledge.Internal;

// [AP.01] sealed default implementation
internal sealed class DefaultKnowledgeFormatter : IVKPromptFormatter
{
    private readonly IVKKnowledgeRenderer _renderer;

    public DefaultKnowledgeFormatter(IVKKnowledgeRenderer renderer)
    {
        _renderer = VKGuard.NotNull(renderer);
    }

    public bool CanFormat(VKPromptFragment fragment)
        => fragment.TierType == VKPromptTierType.Knowledge;

    public VKResult<string> Format(VKPromptFragment fragment, VKWeavingContext context)
    {
        // [AP.01] Boundary check
        VKGuard.NotNull(fragment);
        VKGuard.NotNull(context);

        if (fragment.Metadata is not VKKnowledgeEntry currentEntry)
        {
            return VKResult.Failure<string>(VKKnowledgeErrors.InvalidMetadataType); // [CS.01]
        }

        // Get all active Knowledge fragments sharing the same slot (Role and Depth)
        var disabledTiers = context.Args?.DisabledTiers ?? new List<VKPromptTierType>();
        if (disabledTiers.Contains(VKPromptTierType.Knowledge))
        {
            return VKResult.Success(string.Empty);
        }

        var siblingFragments = context.Fragments
            .Where(f => f.TierType == VKPromptTierType.Knowledge &&
                        f.Metadata is VKKnowledgeEntry siblingEntry &&
                        (
                            (currentEntry.Position is VKKnowledgeRelativePosition currentRel &&
                             siblingEntry.Position is VKKnowledgeRelativePosition siblingRel &&
                             currentRel.Relative == siblingRel.Relative)
                            ||
                            (currentEntry.Position is VKKnowledgeAbsolutePosition currentAbs &&
                             siblingEntry.Position is VKKnowledgeAbsolutePosition siblingAbs &&
                             currentAbs.Role == siblingAbs.Role && currentAbs.Depth == siblingAbs.Depth)
                        ) &&
                        siblingEntry.Tag == currentEntry.Tag)
            .OrderBy(f => f.RenderOrder)
            .ToList();

        if (siblingFragments.Count == 0)
        {
            return VKResult.Success(string.Empty);
        }

        // Only the first fragment in the slot group renders the combined XML; others return Empty to avoid duplicate renders
        var firstFragment = siblingFragments[0];
        if (fragment != firstFragment)
        {
            return VKResult.Success(string.Empty);
        }

        try
        {
            var sb = new System.Text.StringBuilder();
            var firstEntry = (VKKnowledgeEntry)firstFragment.Metadata!;
            string tag = firstEntry.Tag;

            sb.AppendLine($"<{tag}>");

            for (int i = 0; i < siblingFragments.Count; i++)
            {
                var sib = siblingFragments[i];
                var entry = (VKKnowledgeEntry)sib.Metadata!;
                string rendered = _renderer.Render(entry);
                
                if (i > 0)
                {
                    sb.AppendLine();
                }
                
                sb.AppendLine(rendered.TrimEnd());
            }

            sb.Append($"</{tag}>");

            return VKResult.Success(sb.ToString());
        }
        catch (Exception)
        {
            return VKResult.Failure<string>(VKKnowledgeErrors.InvalidMetadataType);
        }
    }
}

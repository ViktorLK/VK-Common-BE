using System.Collections.Generic;
using VK.Blocks.Core;

// // [AP.03] Internal implementation inside Internal/ folder without VK prefix
namespace VK.Blocks.AI.Cognitive.Weaving.Internal;

internal sealed class BasicPromptFormatter : IVKPromptFormatter<VKDefaultModelMarker>
{
    public VKResult<IReadOnlyList<VKFormattedTier>> Format(IReadOnlyList<VKScoredFragment> truncated, VKWeavingContext context)
    {
        VKGuard.NotNull(truncated);
        VKGuard.NotNull(context);

        var formattedTiers = new List<VKFormattedTier>();

        foreach (var item in truncated)
        {
            var fragment = item.Fragment;

            // Standard formatting. Strip think tags if necessary.
            string content = fragment.Content;
            int thinkStart = content.IndexOf("<think>");
            int thinkEnd = content.IndexOf("</think>");

            if (thinkStart >= 0 && thinkEnd > thinkStart)
            {
                // Basic naive strip for standard models
                content = content.Remove(thinkStart, thinkEnd - thinkStart + 8);
            }

            var role = VKChatRole.System;
            if (fragment.Metadata is IDictionary<string, object?> dict && dict.TryGetValue("Role", out var roleObj) && roleObj is VKChatRole r)
            {
                role = r;
            }

            formattedTiers.Add(new VKFormattedTier
            {
                Content = content.Trim(),
                Position = fragment.Position,
                TierType = fragment.TierType,
                Role = role,
                Depth = fragment.Depth
            });
        }

        return VKResult.Success<IReadOnlyList<VKFormattedTier>>(formattedTiers);
    }
}

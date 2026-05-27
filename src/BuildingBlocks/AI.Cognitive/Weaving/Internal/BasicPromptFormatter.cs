using System.Collections.Generic;
using System.Linq;
using VK.Blocks.Core;

// // [AP.03] Internal implementation inside Internal/ folder without VK prefix
namespace VK.Blocks.AI.Cognitive.Weaving.Internal;

internal sealed class BasicPromptFormatter : IVKPromptFormatter<VKDefaultModelMarker>
{
    private readonly IEnumerable<IVKPromptFormatter> _formatters;

    public BasicPromptFormatter(IEnumerable<IVKPromptFormatter> formatters)
    {
        _formatters = VKGuard.NotNull(formatters);
    }

    public VKResult<IReadOnlyList<VKFormattedTier>> FormatContent(
        IReadOnlyList<VKScoredFragment> truncated, 
        VKOrchestrationPipelineContext context)
    {
        VKGuard.NotNull(truncated);
        VKGuard.NotNull(context);

        var formattedTiers = new List<VKFormattedTier>();

        foreach (var item in truncated)
        {
            var fragment = item.Fragment;

            // 1. Check if we have an IVKPromptFormatter for this fragment's type
            var matchedFormatter = _formatters.FirstOrDefault(f => f.CanFormat(fragment));
            if (matchedFormatter is not null)
            {
                var formatResult = matchedFormatter.Format(fragment, context);
                if (formatResult.IsFailure)
                {
                    return VKResult.Failure<IReadOnlyList<VKFormattedTier>>(formatResult.FirstError);
                }
                fragment.Content = formatResult.Value;
            }

            // 2. Standard formatting. Strip think tags if necessary.
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
public interface IVKPromptFormatter<TModel>
{
    VKResult<IReadOnlyList<VKFormattedTier>> FormatContent(IReadOnlyList<VKScoredFragment> truncated, VKOrchestrationPipelineContext context);
}

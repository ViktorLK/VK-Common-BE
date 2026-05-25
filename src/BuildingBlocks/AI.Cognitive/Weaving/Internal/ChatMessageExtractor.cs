using System.Collections.Generic;
using VK.Blocks.Core;

// // [AP.03] Internal implementation inside Internal/ folder without VK prefix
namespace VK.Blocks.AI.Cognitive.Weaving.Internal;

internal sealed class ChatMessageExtractor : IVKPromptExtractor<IEnumerable<VKChatMessage>>
{
    public VKResult<IReadOnlyList<VKPromptFragment>> Extract(IEnumerable<VKChatMessage> source, VKWeavingContext context)
    {
        VKGuard.NotNull(source);
        VKGuard.NotNull(context);

        var fragments = new List<VKPromptFragment>();
        int depth = 0; // Recent messages have lower depth in history typically.

        foreach (var msg in source)
        {
            fragments.Add(new VKPromptFragment
            {
                Id = $"chat_{depth}",
                Content = msg.Content,
                Position = msg.Role == VKChatRole.System ? VKKnowledgePositions.BeforeDefs : VKKnowledgePositions.SystemAtDepth, // Mapped logic for roles
                TierType = msg.Role == VKChatRole.System ? VKPromptTierType.SystemInstructions : VKPromptTierType.ChatHistory,
                Priority = 0,
                Depth = depth,
                Metadata = new Dictionary<string, object?>
                {
                    { "Role", msg.Role },
                    { "OriginalMetadata", msg.Metadata }
                }
            });
            depth++;
        }

        return VKResult.Success<IReadOnlyList<VKPromptFragment>>(fragments);
    }
}

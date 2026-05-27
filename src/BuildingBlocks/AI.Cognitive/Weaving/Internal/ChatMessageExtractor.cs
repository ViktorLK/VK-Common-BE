using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

// // [AP.03] Internal implementation inside Internal/ folder without VK prefix
namespace VK.Blocks.AI.Cognitive.Weaving.Internal;

// [AP.01] sealed default implementation
internal sealed class ChatMessageExtractor : IVKPromptExtractor
{
    public Task<VKResult<IReadOnlyList<VKPromptFragment>>> ExtractAsync(
        VKOrchestrationPipelineContext context,
        CancellationToken ct)
    {
        // [AP.01]
        VKGuard.NotNull(context);

        if (context.Messages is null)
        {
            IReadOnlyList<VKPromptFragment> empty = [];
            return Task.FromResult(VKResult.Success(empty));
        }

        var fragments = new List<VKPromptFragment>();
        int depth = 0; // Recent messages have lower depth in history typically.

        foreach (var msg in context.Messages)
        {
            fragments.Add(new VKPromptFragment
            {
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

        IReadOnlyList<VKPromptFragment> resultList = fragments; // [AP.01]
        return Task.FromResult(VKResult.Success(resultList));
    }
}

using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Knowledge.Internal;

// [AP.01] sealed default implementation
internal sealed class DefaultKnowledgeFormatter : IVKPromptFormatter
{
    public bool CanFormat(VKPromptFragment fragment)
        => fragment.TierType == VKPromptTierType.Knowledge;

    public VKResult<string> Format(VKPromptFragment fragment, VKOrchestrationPipelineContext context)
    {
        // [AP.01] Boundary check
        VKGuard.NotNull(fragment);
        VKGuard.NotNull(context);

        if (fragment.Metadata is not VKKnowledgeEntry entry)
        {
            return VKResult.Failure<string>(VKKnowledgeErrors.InvalidMetadataType); // [CS.01]
        }

        string result =
        $"""
        <knowledge>
          <content>{entry.Content}</content>
        </knowledge>
        """;

        return VKResult.Success(result);
    }
}

using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Knowledge.Internal;

internal sealed class DefaultKnowledgeRenderer : IVKKnowledgeRenderer
{
    public string Render(VKKnowledgeEntry entry)
    {
        VKGuard.NotNull(entry);
        return entry.Segment.Content;
    }
}

using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Echo.Internal;

internal sealed class BracketEchoRenderer : IVKEchoRenderer
{
    public string Render(VKEchoTrace trace)
    {
        VKGuard.NotNull(trace);
        return $"[{trace.Role}]{Weaving.Internal.PsycheConstants.Separators.DefaultRoleHeader}{trace.Content}";
    }
}

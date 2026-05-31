using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Echo.Internal;

internal sealed class DefaultEchoRenderer : IVKEchoRenderer
{
    public string Render(VKEchoTrace trace)
    {
        VKGuard.NotNull(trace);
        return $"{trace.Role}{Weaving.Internal.PromptConstants.Separators.DefaultRoleHeader}{trace.Content}";
    }
}

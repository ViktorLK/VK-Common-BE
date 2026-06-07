using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Echo.Internal;

internal sealed class ChatMLEchoRenderer : IVKEchoRenderer
{
    public string Render(VKEchoTrace trace)
    {
        VKGuard.NotNull(trace);
        string role = trace.Role.ToString().ToLowerInvariant();
        return $"{Weaving.Internal.PsycheConstants.ChatML.ImStart}{role}\n{trace.Content}{Weaving.Internal.PsycheConstants.ChatML.ImEnd}";
    }
}

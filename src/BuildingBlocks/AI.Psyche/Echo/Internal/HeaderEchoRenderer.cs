using VK.Blocks.AI.Psyche.Common.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Echo.Internal;

internal sealed class HeaderEchoRenderer : IVKEchoRenderer
{
    public string Render(VKEchoTrace trace, VKPsycheContext context)
    {
        VKGuard.NotNull(trace);
        VKGuard.NotNull(context);

        string label = trace.Role switch
        {
            VKChatRole.User => context.State<VKUserPresence>()?.DisplayName ?? trace.Role.ToString(),
            VKChatRole.Assistant => context.State<VKPersonaAnchor>()?.Name ?? trace.Role.ToString(),
            _ => trace.Role.ToString()
        };

        return $"{label}{PsycheConstants.Separators.DefaultRoleHeader}{trace.Content}";
    }
}

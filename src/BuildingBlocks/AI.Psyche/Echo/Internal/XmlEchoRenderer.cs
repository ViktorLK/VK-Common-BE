using VK.Blocks.AI.Psyche.Common.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Echo.Internal;

internal sealed class XmlEchoRenderer : IVKEchoRenderer
{
    public string Render(VKEchoTrace trace, VKPsycheContext context)
    {
        VKGuard.NotNull(trace);
        VKGuard.NotNull(context);

        string role = trace.Role.ToString().ToLowerInvariant();
        string tag = PsycheConstants.XmlTags.Message;
        return $"<{tag} role=\"{role}\">{trace.Content}</{tag}>";
    }
}

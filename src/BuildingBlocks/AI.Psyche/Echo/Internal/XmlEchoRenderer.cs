using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Echo.Internal;

internal sealed class XmlEchoRenderer : IVKEchoRenderer
{
    public string Render(VKEchoTrace trace)
    {
        VKGuard.NotNull(trace);
        string role = trace.Role.ToString().ToLowerInvariant();
        string tag = Weaving.Internal.PsycheConstants.XmlTags.Message;
        return $"<{tag} role=\"{role}\">{trace.Content}</{tag}>";
    }
}

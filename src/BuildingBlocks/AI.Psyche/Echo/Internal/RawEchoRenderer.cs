using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Echo.Internal;

/// <summary>
/// Echo renderer that returns the raw trace content without any role formatting or headers.
/// Ideal for structured Chat APIs.
/// Follows AP.01 (sealed class default).
/// </summary>
internal sealed class RawEchoRenderer : IVKEchoRenderer
{
    public string Render(VKEchoTrace trace, VKPsycheContext context)
    {
        VKGuard.NotNull(trace);
        VKGuard.NotNull(context);

        return trace.Content;
    }
}

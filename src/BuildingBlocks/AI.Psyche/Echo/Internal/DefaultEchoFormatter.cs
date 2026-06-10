using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Echo.Internal;

// [AP.01] sealed default implementation
internal sealed class DefaultEchoFormatter : IVKPromptFormatter
{
    private readonly IVKEchoRenderer _renderer;

    public DefaultEchoFormatter(IVKEchoRenderer renderer)
    {
        _renderer = VKGuard.NotNull(renderer);
    }

    public bool CanFormat(VKPromptFragment fragment)
        => fragment.TierType == VKPromptTierType.Echo;

    public VKResult<string> Format(VKPromptFragment fragment, VKPsycheContext context)
    {
        // [AP.01] Boundary check
        VKGuard.NotNull(fragment);
        VKGuard.NotNull(context);

        if (fragment.Metadata is not VKEchoTrace trace)
        {
            return VKResult.Failure<string>(VKError.Failure("AI.Echo.InvalidMetadataType", "The metadata provided to the formatter was not of the expected VKEchoTrace type.")); // [CS.01]
        }

        try
        {
            string formatted = _renderer.Render(trace);
            return VKResult.Success(formatted);
        }
        catch (Exception)
        {
            return VKResult.Failure<string>(VKError.Failure("AI.Echo.FormattingFailed", "Formatting dialogue history trace failed."));
        }
    }
}

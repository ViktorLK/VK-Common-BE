using System;
using System.Text;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Persona.Internal;

// [AP.01] sealed default implementation
internal sealed class DefaultPersonaFormatter : IVKPromptFormatter
{
    private readonly IVKPersonaRenderer _renderer;

    public DefaultPersonaFormatter(IVKPersonaRenderer renderer)
    {
        _renderer = VKGuard.NotNull(renderer);
    }

    public bool CanFormat(VKPromptFragment fragment)
        => fragment.TierType == VKPromptTierType.Persona;

    public VKResult<string> Format(VKPromptFragment fragment, VKWeavingContext context)
    {
        // [AP.01] Boundary check
        VKGuard.NotNull(fragment);
        VKGuard.NotNull(context);

        if (fragment.Metadata is not VKPersonaAnchor persona)
        {
            return VKResult.Failure<string>(VKPersonaErrors.InvalidMetadataType); // [CS.01]
        }

        try
        {
            // High performance rendering path using StringBuilder with capacity estimation [AP.01]
            var sb = new StringBuilder(512);

            // 1. Macro Skeleton & Sovereignty (L1/L4) -> XML Tag
            sb.AppendLine($"<{VK.Blocks.AI.Psyche.Weaving.Internal.PsycheConstants.XmlTags.Persona}>");

            // 2. Delegate the actual markdown content rendering to IVKPersonaRenderer
            sb.Append(_renderer.Render(persona));

            sb.AppendLine($"</{VK.Blocks.AI.Psyche.Weaving.Internal.PsycheConstants.XmlTags.Persona}>");

            return VKResult.Success(sb.ToString());
        }
        catch (Exception)
        {
            return VKResult.Failure<string>(VKPersonaErrors.FormattingFailed);
        }
    }
}

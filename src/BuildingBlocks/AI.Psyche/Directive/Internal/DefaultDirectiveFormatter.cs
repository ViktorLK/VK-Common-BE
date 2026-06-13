using System.Text;
using VK.Blocks.AI.Psyche.Common.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Directive.Internal;

// [AP.01] sealed default implementation
internal sealed class DefaultDirectiveFormatter : IVKPromptFormatter
{
    public bool CanFormat(VKPromptFragment fragment)
        => fragment.TierType == VKPromptTierType.Directive;

    public VKResult<string> Format(VKPromptFragment fragment, VKPsycheContext context)
    {
        // [AP.01] Boundary check
        VKGuard.NotNull(fragment);
        VKGuard.NotNull(context);

        if (fragment.Metadata is not VKDirectiveCharter directive)
        {
            return VKResult.Failure<string>(VKDirectiveErrors.InvalidMetadataType); // [CS.01]
        }

        var sb = new StringBuilder(512);
        sb.AppendLine($"<{PsycheConstants.XmlTags.SystemDirectives}>");

        if (!string.IsNullOrWhiteSpace(directive.BehaviorRules))
            sb.AppendLine(directive.BehaviorRules);
        if (!string.IsNullOrWhiteSpace(directive.SafetyRules))
            sb.AppendLine(directive.SafetyRules);
        if (!string.IsNullOrWhiteSpace(directive.OutputConstraints))
            sb.AppendLine(directive.OutputConstraints);
        if (!string.IsNullOrWhiteSpace(directive.Overview))
            sb.AppendLine(directive.Overview);

        sb.AppendLine($"</{PsycheConstants.XmlTags.SystemDirectives}>");

        return VKResult.Success(sb.ToString().Trim());
    }
}

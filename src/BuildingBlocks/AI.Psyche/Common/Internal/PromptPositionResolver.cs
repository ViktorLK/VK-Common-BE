using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Common.Internal;

/// <summary>
/// Static helper to resolve relative positions of prompt entries into unified prompt coordinates.
/// </summary>
internal static class PromptPositionResolver
{
    internal readonly record struct FragmentCoordinate(
        VKChatRole Role,
        int RenderOrder);

    internal static FragmentCoordinate Resolve(
        VKPromptSegment segment,
        IReadOnlyDictionary<VKPromptTierType, int> renderOrders)
    {
        VKGuard.NotNull(segment);
        return segment.AbsoluteDepth is not null
            ? new FragmentCoordinate(segment.Role, segment.Priority)
            : new FragmentCoordinate(VKChatRole.System, ResolveRelativeOrder(segment.Anchor ?? VKPromptRelativeAnchor.AfterPersona, renderOrders, segment.Priority));
    }

    private static int ResolveRelativeOrder(
        VKPromptRelativeAnchor anchor,
        IReadOnlyDictionary<VKPromptTierType, int> renderOrders,
        int priority)
    {
        int directiveBase = renderOrders.GetValueOrDefault(VKPromptTierType.Directive, (int)VKPromptTierType.Directive * 10000);
        int personaBase = renderOrders.GetValueOrDefault(VKPromptTierType.Persona, (int)VKPromptTierType.Persona * 10000);
        int echoBase = renderOrders.GetValueOrDefault(VKPromptTierType.Echo, (int)VKPromptTierType.Echo * 10000);

        int baseOrder = anchor switch
        {
            VKPromptRelativeAnchor.BeforeDirective => directiveBase - PsycheConstants.Layout.RelativeOffset,
            VKPromptRelativeAnchor.AfterDirective => directiveBase + PsycheConstants.Layout.RelativeOffset,
            VKPromptRelativeAnchor.BeforePersona => personaBase - PsycheConstants.Layout.RelativeOffset,
            VKPromptRelativeAnchor.AfterPersona => personaBase + PsycheConstants.Layout.RelativeOffset,
            VKPromptRelativeAnchor.BeforeEcho => echoBase - PsycheConstants.Layout.RelativeOffset,
            VKPromptRelativeAnchor.AfterEcho => echoBase + PsycheConstants.Layout.EchoReserve,
            _ => throw new System.ArgumentOutOfRangeException(nameof(anchor), anchor, $"Unsupported relative anchor value: {anchor}")
        };

        return baseOrder + priority;
    }
}

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
            ? new FragmentCoordinate(segment.Role, segment.DepthPriority)
            : new FragmentCoordinate(VKChatRole.System, ResolveRelativeOrder(segment.RelativeDepth ?? VKPromptRelativeDepth.AfterPersona, renderOrders, segment.DepthPriority));
    }

    private static int ResolveRelativeOrder(
        VKPromptRelativeDepth anchor,
        IReadOnlyDictionary<VKPromptTierType, int> renderOrders,
        int priority)
    {
        int directiveBase = renderOrders.GetValueOrDefault(VKPromptTierType.Directive, (int)VKPromptTierType.Directive * 10000);
        int personaBase = renderOrders.GetValueOrDefault(VKPromptTierType.Persona, (int)VKPromptTierType.Persona * 10000);
        int echoBase = renderOrders.GetValueOrDefault(VKPromptTierType.Echo, (int)VKPromptTierType.Echo * 10000);

        int baseOrder = anchor switch
        {
            VKPromptRelativeDepth.BeforeDirective => directiveBase - PsycheConstants.Layout.RelativeOffset,
            VKPromptRelativeDepth.AfterDirective => directiveBase + PsycheConstants.Layout.RelativeOffset,
            VKPromptRelativeDepth.BeforePersona => personaBase - PsycheConstants.Layout.RelativeOffset,
            VKPromptRelativeDepth.AfterPersona => personaBase + PsycheConstants.Layout.RelativeOffset,
            VKPromptRelativeDepth.BeforeEcho => echoBase - PsycheConstants.Layout.RelativeOffset,
            VKPromptRelativeDepth.AfterEcho => echoBase + PsycheConstants.Layout.EchoReserve,
            _ => throw new System.ArgumentOutOfRangeException(nameof(anchor), anchor, $"Unsupported relative anchor value: {anchor}")
        };

        return baseOrder + priority;
    }
}

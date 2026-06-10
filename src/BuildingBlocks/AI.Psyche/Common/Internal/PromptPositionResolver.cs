using System.Collections.Generic;

namespace VK.Blocks.AI.Psyche.Common.Internal;

/// <summary>
/// Static helper to resolve absolute and relative positions of prompt entries into unified prompt coordinates.
/// </summary>
internal static class PromptPositionResolver
{
    internal readonly record struct FragmentCoordinate(
        VKChatRole Role,
        int? Depth,
        int RenderOrder);

    internal static FragmentCoordinate Resolve(
        IVKPromptPosition position,
        int priority,
        IReadOnlyDictionary<VKPromptTierType, int> renderOrders)
    {
        return position switch
        {
            VKAbsolutePromptPosition abs =>
                new FragmentCoordinate(abs.Role, abs.Depth, priority),

            VKRelativePromptPosition rel =>
                new FragmentCoordinate(VKChatRole.System, null, ResolveRelativeOrder(rel, renderOrders, priority)),

            _ => throw new System.ArgumentException($"Unsupported prompt position type: {position?.GetType().Name}", nameof(position))
        };
    }

    private static int ResolveRelativeOrder(
        VKRelativePromptPosition rel,
        IReadOnlyDictionary<VKPromptTierType, int> renderOrders,
        int priority)
    {
        int directiveBase = renderOrders.GetValueOrDefault(VKPromptTierType.Directive, 10000);
        int personaBase = renderOrders.GetValueOrDefault(VKPromptTierType.Persona, 20000);
        int knowledgeBase = renderOrders.GetValueOrDefault(VKPromptTierType.Knowledge, 30000);
        int echoBase = renderOrders.GetValueOrDefault(VKPromptTierType.Echo, 50000);

        int baseOrder = rel.Relative switch
        {
            VKPromptRelativeAnchor.BeforeDirective => directiveBase - 1000,
            VKPromptRelativeAnchor.AfterDirective => directiveBase + 1000,
            VKPromptRelativeAnchor.BeforePersona => personaBase - 1000,
            VKPromptRelativeAnchor.AfterPersona => personaBase + 1000,
            VKPromptRelativeAnchor.BeforeEcho => echoBase - 1000,
            VKPromptRelativeAnchor.AfterEcho => echoBase + 1000,
            _ => knowledgeBase
        };

        return baseOrder + priority;
    }
}

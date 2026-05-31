using System.Collections.Generic;

namespace VK.Blocks.AI.Psyche.Knowledge.Internal;

/// <summary>
/// Static helper to resolve absolute and relative positions of knowledge entries into unified prompt coordinates.
/// Complies with AP.01 and AP.03.
/// </summary>
internal static class KnowledgePositionResolver
{
    internal readonly record struct FragmentCoordinate(
        VKChatRole Role,
        int? Depth,
        int RenderOrder);

    internal static FragmentCoordinate Resolve(
        VKKnowledgeEntry entry,
        IReadOnlyDictionary<VKPromptTierType, int> renderOrders)
    {
        // // [AP.01] Target-typed switch expression & VKGuard at boundary (implicit via entry usage)
        return entry.Position switch
        {
            VKKnowledgeAbsolutePosition abs =>
                new FragmentCoordinate(abs.Role, abs.Depth, entry.Priority),

            VKKnowledgeRelativePosition rel =>
                new FragmentCoordinate(VKChatRole.System, null, ResolveRelativeOrder(rel, renderOrders, entry.Priority)),

            _ => new FragmentCoordinate(VKChatRole.System, null,
                renderOrders.GetValueOrDefault(VKPromptTierType.Knowledge, 2000) + entry.Priority)
        };
    }

    private static int ResolveRelativeOrder(
        VKKnowledgeRelativePosition rel,
        IReadOnlyDictionary<VKPromptTierType, int> renderOrders,
        int priority)
    {
        int directiveBase = renderOrders.GetValueOrDefault(VKPromptTierType.Directive, 0);
        int personaBase = renderOrders.GetValueOrDefault(VKPromptTierType.Persona, 1000);
        int knowledgeBase = renderOrders.GetValueOrDefault(VKPromptTierType.Knowledge, 2000);
        int echoBase = renderOrders.GetValueOrDefault(VKPromptTierType.Echo, 3000);

        int baseOrder = rel.Relative switch
        {
            VKKnowledgeRelative.BeforeDirective => directiveBase - 100,
            VKKnowledgeRelative.AfterDirective => directiveBase + 100,
            VKKnowledgeRelative.BeforePersona => personaBase - 100,
            VKKnowledgeRelative.AfterPersona => personaBase + 100,
            VKKnowledgeRelative.BeforeEcho => echoBase - 100,
            VKKnowledgeRelative.AfterEcho => echoBase + 100,
            _ => knowledgeBase
        };

        return baseOrder + priority;
    }
}

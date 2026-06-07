using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Public diagnostic tokens for the Directive feature.
/// </summary>
public static class VKDirectiveDiagnostic
{
    // Logs (Event IDs mapped on Memory block offset range + offset)
    public const int DirectiveInitializedEventId = VKDiagnosticOffsets.AI_Afferent_Memory + 201;
    public const int DirectiveResolvedEventId = VKDiagnosticOffsets.AI_Afferent_Memory + 202;
}

using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Public diagnostic tokens for the Directive feature.
/// </summary>
public static class VKDirectiveDiagnosticsConstants
{
    // Logs (Event IDs mapped on Memory block offset range + offset)
    public static class Logs
    {
        public const int DirectiveInitialized = VKDiagnosticOffsets.AI_Psyche_Directive + 1;
        public const int DirectiveResolved = VKDiagnosticOffsets.AI_Psyche_Directive + 2;
    }
}

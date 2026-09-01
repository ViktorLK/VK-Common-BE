using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Public diagnostics constants for the Directive feature.
/// Follows BB.04 and OR.01.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Static diagnostics and telemetry constants.")]
public static class VKDirectiveDiagnosticsConstants
{
    // Logs (Event IDs: 72000 - 72999)
    public static class Logs
    {
        public const int DirectiveInitialized = VKDiagnosticOffsets.AI_Psyche_Directive + 1;
        public const int DirectiveResolved = VKDiagnosticOffsets.AI_Psyche_Directive + 2;
    }

    public static class Metrics
    {
        public const string DirectiveStageDuration = "vk.ai.psyche.directive.duration";
        public const string DirectivesResolvedCount = "vk.ai.psyche.directive.resolved_count";
    }

    public static class Tags
    {
        public const string StageName = "ai.psyche.stage";
        public const string DirectiveId = "ai.psyche.directive.id";
    }
}

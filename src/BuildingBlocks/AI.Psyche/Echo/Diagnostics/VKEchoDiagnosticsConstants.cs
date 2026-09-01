using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Public diagnostics constants for the Echo feature.
/// Follows BB.04 and OR.01.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Static diagnostics and telemetry constants.")]
public static class VKEchoDiagnosticsConstants
{
    // Logs (Event IDs: 75000 - 75999)
    public static class Logs
    {
        public const int EchoInitialized = VKDiagnosticOffsets.AI_Psyche_Echo + 1;
        public const int EchoRecorded = VKDiagnosticOffsets.AI_Psyche_Echo + 2;
        public const int EchoTrimmed = VKDiagnosticOffsets.AI_Psyche_Echo + 3;
    }

    // Metrics (Meter/Counter/Histogram Names)
    public static class Metrics
    {
        public const string EchoExtractDuration = "vk.ai.psyche.echo.extract.duration";
        public const string EchoSaveDuration = "vk.ai.psyche.echo.save.duration";
        public const string ActiveEchoesCount = "vk.ai.psyche.echo.active_count";
        public const string TrimmedEchoesCount = "vk.ai.psyche.echo.trimmed_count";
    }

    // Tags
    public static class Tags
    {
        public const string StageName = "ai.psyche.stage";
        public const string RetainedCount = "ai.psyche.echo.retained_count";
        public const string TrimmedCount = "ai.psyche.echo.trimmed_count";
    }
}

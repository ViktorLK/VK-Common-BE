using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Public diagnostic tokens for the Echo feature.
/// </summary>
public static class VKEchoDiagnosticsConstants
{
    // Logs (Event IDs mapped on Memory block offset range)
    public static class Logs
    {
        public const int EchoInitialized = VKDiagnosticOffsets.AI_Psyche_Echo + 1;
        public const int EchoRecorded = VKDiagnosticOffsets.AI_Psyche_Echo + 2;
        public const int EchoTrimmed = VKDiagnosticOffsets.AI_Psyche_Echo + 3;
    }

    // Metrics (Meter/Counter/Histogram Names)
    public static class Metrics
    {
        public const string ActiveEchoesCount = "vk.ai.psyche.echo.active_count";
        public const string TrimmedEchoesCount = "vk.ai.psyche.echo.trimmed_count";
    }
}

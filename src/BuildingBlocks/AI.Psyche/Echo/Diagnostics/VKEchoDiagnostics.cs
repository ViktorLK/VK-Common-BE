using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Public diagnostic tokens for the Echo feature.
/// </summary>
public static class VKEchoDiagnostics
{
    // Logs (Event IDs mapped on Memory block offset range)
    public const int EchoInitializedEventId = VKDiagnosticOffsets.AI_Afferent_Memory + 101;
    public const int EchoRecordedEventId = VKDiagnosticOffsets.AI_Afferent_Memory + 102;
    public const int EchoTrimmedEventId = VKDiagnosticOffsets.AI_Afferent_Memory + 103;

    // Metrics (Meter/Counter/Histogram Names)
    public static class Metrics
    {
        public const string ActiveEchoesCount = "vk.ai.psyche.echo.active_count";
        public const string TrimmedEchoesCount = "vk.ai.psyche.echo.trimmed_count";
    }
}

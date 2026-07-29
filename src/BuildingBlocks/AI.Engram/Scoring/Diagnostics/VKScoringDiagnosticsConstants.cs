using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Scoring.Diagnostics;

/// <summary>
/// Diagnostic constants for Scoring operations.
/// </summary>
[VKBlockDiagnostics<VKAIEngramBlock>]
public static partial class VKScoringDiagnosticsConstants
{
    public const string MeterName = "VK.Blocks.AI.Engram.Scoring";

    public const int ScoringCycleCompletedEventId = 500;
    public const int ScoringEntryEvaluatedEventId = 501;
    public const int ScoringBaseImportanceOverriddenEventId = 502;
    public const int ScoringSecurityRejectedEventId = 503;
    public const int ScoringRoutedToStructuredEventId = 504;
}

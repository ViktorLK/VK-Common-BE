using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Reclamation.Diagnostics;

/// <summary>
/// Diagnostic constants for Reclamation operations.
/// </summary>
[VKBlockDiagnostics<VKAIEngramBlock>]
public static partial class VKReclamationDiagnosticsConstants
{
    public const string MeterName = "VK.Blocks.AI.Engram.Reclamation";

    public const int ReclamationCycleStartedEventId = 600;
    public const int ReclamationCycleCompletedEventId = 601;
    public const int ReclamationCycleErrorEventId = 602;
    public const int ReclamationDecayEvaluatedEventId = 603;
    public const int ReclamationPruneExecutedEventId = 604;
    public const int ReclamationVectorStoreCleanedEventId = 605;
}

namespace VK.Blocks.AI.Engram.Consolidation.Diagnostics;

/// <summary>
/// Diagnostic EventId constants for the Consolidation slice.
/// </summary>
public static class VKConsolidationDiagnosticsConstants
{
    public const int IdempotencySkippedEventId = 401;
    public const int PoisoningGuardSkippedSizeEventId = 402;
    public const int PoisoningGuardSkippedInjectionEventId = 403;
    public const int ContradictionArbitratedEventId = 404;
    public const int PersistenceFailedDlqEventId = 405;
    public const int ConsolidationCompletedEventId = 406;
    public const int DeduplicationMergedEventId = 407;
    public const int DeduplicationDroppedEventId = 408;
    public const int VectorIndexedEventId = 409;
    public const int PersistenceRetryEventId = 410;
}

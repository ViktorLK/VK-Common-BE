namespace VK.Blocks.AI.Engram.Compression.Diagnostics;

/// <summary>
/// Diagnostic constants for the Compression slice.
/// </summary>
public static class VKCompressionDiagnosticsConstants
{
    public const int CompressionTriggeredEventId = 301;
    public const int CompressionCompletedEventId = 302;
    public const int CompressionSkippedEventId = 303;
    public const int CompressionFailedEventId = 304;
    public const int L2CompressionTriggeredEventId = 305;
    public const int CompressionSkippedLockBusyEventId = 306;

    public const int JobEnqueuedEventId = 311;
    public const int QueueFullEventId = 312;

    public const int WorkerDisabledEventId = 321;
    public const int WorkerStartedEventId = 322;
    public const int WorkerStoppedEventId = 323;
    public const int CycleStartingEventId = 324;
    public const int CycleCompletedEventId = 325;
    public const int SessionsFoundEventId = 326;
    public const int CycleErrorEventId = 327;
    public const int SearchFailedEventId = 328;
    public const int SessionCompressionFailedEventId = 329;
    public const int SessionExceptionEventId = 330;

    public const int TopicSegmentationFailedEventId = 341;
    public const int NoValidSegmentsParsedEventId = 342;
}

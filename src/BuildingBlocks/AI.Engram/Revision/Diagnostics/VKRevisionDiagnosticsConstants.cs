using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Revision.Diagnostics;

/// <summary>
/// Diagnostic constants for Revision operations.
/// </summary>
[VKBlockDiagnostics<VKAIEngramBlock>]
public static partial class VKRevisionDiagnosticsConstants
{
    public const string MeterName = "VK.Blocks.AI.Engram.Revision";

    public const int RevisionArbitrationCompletedEventId = 700;
    public const int RevisionArbitrationErrorEventId = 701;
    public const int RevisionUpdatesThrottledEventId = 702;
    public const int RevisionEntryUpdatedEventId = 703;
    public const int RevisionContradictionLoggedEventId = 704;
    public const int RevisionSkippedIdempotentEventId = 705;
    public const int RevisionRejectedLowerAuthorityEventId = 706;
    public const int RevisionRollbackCompletedEventId = 707;
    public const int SynopsisMarkedStaleEventId = 708;
}

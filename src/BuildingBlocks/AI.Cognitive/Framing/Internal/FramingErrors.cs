using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Framing.Internal;

/// <summary>
/// Predefined error constants for the Cognitive Framing slice.
/// Follows CS.01.
/// </summary>
internal static class FramingErrors
{
    public static readonly VKError GovernanceSnapshotMissing = VKError.Validation(
        "Framing.Snapshot.Missing",
        "Governance snapshot is missing from orchestration context. Ensure PresenceGovernancePipelineStage runs first.");
}

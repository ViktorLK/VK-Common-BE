using Microsoft.Extensions.Logging;

namespace VK.Blocks.AI.Engram.Scoring.Diagnostics.Internal;

internal static partial class ScoringDiagnostics
{
    [LoggerMessage(
        EventId = VKScoringDiagnosticsConstants.ScoringCycleCompletedEventId,
        Level = LogLevel.Information,
        Message = "Scoring stage completed: evaluated {EvaluatedCount} entries.")]
    public static partial void ScoringCycleCompleted(this ILogger logger, int evaluatedCount);

    [LoggerMessage(
        EventId = VKScoringDiagnosticsConstants.ScoringEntryEvaluatedEventId,
        Level = LogLevel.Debug,
        Message = "Scored memory entry {EntryId} with BaseImportance {BaseImportance:F4} (Category={Category}).")]
    public static partial void ScoringEntryEvaluated(this ILogger logger, string entryId, double baseImportance, string category);

    [LoggerMessage(
        EventId = VKScoringDiagnosticsConstants.ScoringBaseImportanceOverriddenEventId,
        Level = LogLevel.Information,
        Message = "Manually overridden BaseImportance for memory entry {EntryId} from {OldValue:F4} to {NewValue:F4}.")]
    public static partial void ScoringBaseImportanceOverridden(this ILogger logger, string entryId, float oldValue, double newValue);

    [LoggerMessage(
        EventId = VKScoringDiagnosticsConstants.ScoringSecurityRejectedEventId,
        Level = LogLevel.Warning,
        Message = "Memory entry {EntryId} rejected by security rule. Reason: {Reason}")]
    public static partial void ScoringSecurityRejected(this ILogger logger, string entryId, string? reason);

    [LoggerMessage(
        EventId = VKScoringDiagnosticsConstants.ScoringRoutedToStructuredEventId,
        Level = LogLevel.Information,
        Message = "Memory entry {EntryId} routed to IVKMemoryStructured KV store with key: {FactKey}")]
    public static partial void ScoringRoutedToStructured(this ILogger logger, string entryId, string factKey);
}

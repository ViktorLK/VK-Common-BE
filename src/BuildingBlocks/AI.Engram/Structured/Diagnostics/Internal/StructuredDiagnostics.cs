using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Structured.Diagnostics.Internal;

[VKBlockDiagnostics<VKAIEngramBlock>]
internal static partial class StructuredDiagnostics
{
    [LoggerMessage(
        EventId = VKStructuredDiagnosticsConstants.FactStoredEventId,
        Level = LogLevel.Information,
        Message = "Structured Fact stored. Key: {Key}, TenantId: {TenantId}, IsSensitive: {IsSensitive}")]
    public static partial void FactStored(this ILogger logger, string key, string? tenantId, bool isSensitive);

    [LoggerMessage(
        EventId = VKStructuredDiagnosticsConstants.FactRemovedEventId,
        Level = LogLevel.Information,
        Message = "Structured Fact explicitly removed (GDPR/Forget). Key: {Key}, TenantId: {TenantId}")]
    public static partial void FactRemoved(this ILogger logger, string key, string? tenantId);

    [LoggerMessage(
        EventId = VKStructuredDiagnosticsConstants.FactConflictResolvedEventId,
        Level = LogLevel.Information,
        Message = "Structured Fact conflict resolved via LWW. Key: {Key}, OldValue: {OldValueMasked}, NewValue: {NewValueMasked}, TenantId: {TenantId}")]
    public static partial void FactConflictResolved(this ILogger logger, string key, string oldValueMasked, string newValueMasked, string? tenantId);

    [LoggerMessage(
        EventId = VKStructuredDiagnosticsConstants.FactTypeMismatchEventId,
        Level = LogLevel.Warning,
        Message = "Structured Fact type mismatch on retrieve. Key: {Key}, ExpectedType: {ExpectedType}, ActualType: {ActualType}")]
    public static partial void FactTypeMismatch(this ILogger logger, string key, string expectedType, string actualType);
}

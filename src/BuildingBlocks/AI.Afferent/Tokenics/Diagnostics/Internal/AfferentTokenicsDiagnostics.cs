using System;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Afferent;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.Tokenics.Diagnostics.Internal;

/// <summary>
/// Source-generated high-performance loggers and diagnostics for the Tokenics slice.
/// Follows OR.01.
/// </summary>
[VKBlockDiagnostics<VKAIAfferentBlock>]
internal static partial class AfferentTokenicsDiagnostics
{
    [LoggerMessage(
        EventId = VKAfferentTokenicsDiagnosticTokens.TokenicsPipelineStartedEventId,
        Level = LogLevel.Information,
        Message = "Afferent Tokenics stage initiated for TenantId: {TenantId}, UserId: {UserId}.")]
    public static partial void TokenicsPipelineStarted(ILogger logger, string tenantId, string userId);

    [LoggerMessage(
        EventId = VKAfferentTokenicsDiagnosticTokens.TokenicsPipelineCompletedEventId,
        Level = LogLevel.Information,
        Message = "Afferent Tokenics stage successfully completed. Counted Tokens: {TokenCount}, Max Budget: {MaxBudget}.")]
    public static partial void TokenicsPipelineCompleted(ILogger logger, int tokenCount, int maxBudget);

    [LoggerMessage(
        EventId = VKAfferentTokenicsDiagnosticTokens.TokenicsBudgetExceededEventId,
        Level = LogLevel.Warning,
        Message = "Afferent Tokenics budget exceeded! Counted Tokens: {TokenCount}, Max Budget: {MaxBudget}. Enforce: {EnforceLimit}")]
    public static partial void TokenicsBudgetExceeded(ILogger logger, int tokenCount, int maxBudget, bool enforceLimit);
}

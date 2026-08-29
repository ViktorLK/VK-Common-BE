using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Profile.Diagnostics.Internal;

/// <summary>
/// Source-generated logger messages and metrics for Psyche Profile stage.
/// Follows BB.04 and OR.01.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Source-generated diagnostics logger declarations containing no business logic.")]
[VKBlockDiagnostics<VKAIPsycheBlock>]
internal static partial class ProfileDiagnostics
{
    // --- Source Generated Metrics (v1.5) ---

    [VKMetricHistogram(
        VKProfileDiagnosticsConstants.Metrics.ProfileStageDuration,
        Unit = "ms",
        Description = "Duration of profile resolution and injection in milliseconds.")]
    public static partial void RecordProfileStage(
        double durationMs,
        [VKMetricTag(VKProfileDiagnosticsConstants.Tags.StageName)] string stage,
        [VKMetricTag(VKPsycheDiagnosticsConstants.Tags.IsSuccess)] bool success);

    [VKMetricCounter(
        VKProfileDiagnosticsConstants.Metrics.ProfilesResolvedCount,
        Unit = "profiles",
        Description = "Total number of user profiles resolved and injected into prompt context.")]
    public static partial void RecordProfilesResolved(
        long count,
        [VKMetricTag(VKProfileDiagnosticsConstants.Tags.StageName)] string stage);

    // --- [LoggerMessage] Generators (OR.01) ---

    [LoggerMessage(
        EventId = VKProfileDiagnosticsConstants.Logs.ProfileInitialized,
        Level = LogLevel.Information,
        Message = "Profile provider initialized.")]
    public static partial void ProfileInitialized(this ILogger logger);

    [LoggerMessage(
        EventId = VKProfileDiagnosticsConstants.Logs.ProfileResolved,
        Level = LogLevel.Debug,
        Message = "Resolved Profile {ProfileId}. PreferredLanguage: {Language}, TimeZone: {TimeZone}")]
    public static partial void ProfileResolved(this ILogger logger, string profileId, string language, string timeZone);
}

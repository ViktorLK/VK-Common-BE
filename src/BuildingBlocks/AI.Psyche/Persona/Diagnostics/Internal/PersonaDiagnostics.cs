using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Persona.Diagnostics.Internal;

/// <summary>
/// Source-generated logger messages and metrics for Psyche Persona stage.
/// Follows BB.04 and OR.01.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Source-generated diagnostics logger declarations containing no business logic.")]
[VKBlockDiagnostics<VKAIPsycheBlock>]
internal static partial class PersonaDiagnostics
{
    // --- Source Generated Metrics (v1.5) ---

    [VKMetricHistogram(
        VKPersonaDiagnosticsConstants.Metrics.PersonaStageDuration,
        Unit = "ms",
        Description = "Duration of persona resolution and injection in milliseconds.")]
    public static partial void RecordPersonaStage(
        double durationMs,
        [VKMetricTag(VKPersonaDiagnosticsConstants.Tags.StageName)] string stage,
        [VKMetricTag(VKPsycheDiagnosticsConstants.Tags.IsSuccess)] bool success);

    [VKMetricCounter(
        VKPersonaDiagnosticsConstants.Metrics.PersonasResolvedCount,
        Unit = "personas",
        Description = "Total number of personas resolved and injected into prompt context.")]
    public static partial void RecordPersonasResolved(
        long count,
        [VKMetricTag(VKPersonaDiagnosticsConstants.Tags.StageName)] string stage);

    // --- [LoggerMessage] Generators (OR.01) ---

    [LoggerMessage(
        EventId = VKPersonaDiagnosticsConstants.Logs.PersonaResolved,
        Level = LogLevel.Debug,
        Message = "Persona anchor resolved: {PersonaId} ({Name})")]
    public static partial void PersonaResolved(this ILogger logger, VKPersonaId personaId, string name);

    [LoggerMessage(
        EventId = VKPersonaDiagnosticsConstants.Logs.PersonaRendered,
        Level = LogLevel.Debug,
        Message = "Persona system prompt rendered for {PersonaId} ({Length} chars)")]
    public static partial void PersonaRendered(this ILogger logger, VKPersonaId personaId, int length);
}

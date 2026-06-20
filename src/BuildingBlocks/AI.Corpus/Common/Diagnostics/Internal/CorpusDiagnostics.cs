using System.Diagnostics;
using System.Diagnostics.Metrics;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Diagnostics.Internal;

/// <summary>
/// Diagnostic signals and OpenTelemetry hook definitions for the AI.Corpus block.
/// Follows OR.01 / BB.04.
/// </summary>
[VKBlockDiagnostics<VKAICorpusBlock>]
internal static partial class CorpusDiagnostics
{
    private static readonly Counter<long> GatheringCandidates;
    private static readonly Histogram<double> GatheringDuration;

    private static readonly Counter<long> FilteringPassed;
    private static readonly Counter<long> FilteringTotal;
    private static readonly Histogram<double> FilteringDuration;
    private static readonly Counter<long> FilterEvaluations;

    private static readonly Counter<long> TrackingInjections;
    private static readonly Histogram<double> TrackingDuration;

    static CorpusDiagnostics()
    {
        GatheringCandidates = Meter.CreateCounter<long>(DiagnosticsConstants.Metrics.GatheringCandidateCount);
        GatheringDuration = Meter.CreateHistogram<double>(DiagnosticsConstants.Metrics.GatheringDuration, "ms");
        FilteringPassed = Meter.CreateCounter<long>(DiagnosticsConstants.Metrics.FilteringPassedCount);
        FilteringTotal = Meter.CreateCounter<long>(DiagnosticsConstants.Metrics.FilteringTotalCount);
        FilteringDuration = Meter.CreateHistogram<double>(DiagnosticsConstants.Metrics.FilteringDuration, "ms");
        FilterEvaluations = Meter.CreateCounter<long>(DiagnosticsConstants.Metrics.FilterEvaluationCount);
        TrackingInjections = Meter.CreateCounter<long>(DiagnosticsConstants.Metrics.TrackingInjectionCount);
        TrackingDuration = Meter.CreateHistogram<double>(DiagnosticsConstants.Metrics.TrackingDuration, "ms");
    }

    public static void RecordGathering(string sessionId, int candidateCount, double durationMs)
    {
        TagList tags = new()
        {
            { DiagnosticsConstants.Tags.SessionId, sessionId },
            { DiagnosticsConstants.Tags.StageName, DiagnosticsConstants.Activities.Gathering }
        };

        GatheringCandidates.Add(candidateCount, tags);
        GatheringDuration.Record(durationMs, tags);
    }

    public static void RecordFiltering(string sessionId, int passedCount, int totalCount, double durationMs)
    {
        TagList tags = new()
        {
            { DiagnosticsConstants.Tags.SessionId, sessionId },
            { DiagnosticsConstants.Tags.StageName, DiagnosticsConstants.Activities.Filtering }
        };

        FilteringPassed.Add(passedCount, tags);
        FilteringTotal.Add(totalCount, tags);
        FilteringDuration.Record(durationMs, tags);
    }

    public static void RecordFilterEvaluation(string filterName, string verdict)
    {
        TagList tags = new()
        {
            { DiagnosticsConstants.Tags.FilterName, filterName },
            { DiagnosticsConstants.Tags.FilterVerdict, verdict }
        };

        FilterEvaluations.Add(1, tags);
    }

    public static void RecordTracking(string sessionId, int injectionCount, double durationMs)
    {
        TagList tags = new()
        {
            { DiagnosticsConstants.Tags.SessionId, sessionId },
            { DiagnosticsConstants.Tags.StageName, DiagnosticsConstants.Activities.Tracking }
        };

        TrackingInjections.Add(injectionCount, tags);
        TrackingDuration.Record(durationMs, tags);
    }
}

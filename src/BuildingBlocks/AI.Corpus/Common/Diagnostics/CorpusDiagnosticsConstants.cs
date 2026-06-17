namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Semantic tokens for AI.Corpus OpenTelemetry instrumentation.
/// Follows BB.04 / OR.01.
/// </summary>
internal static class CorpusDiagnosticsConstants
{
    // ── Tags ──
    public static class Tags
    {
        public const string SessionId     = "vk.corpus.session_id";
        public const string StageName     = "vk.corpus.stage.name";
        public const string Success       = "vk.corpus.success";
        public const string FilterName    = "vk.corpus.filter.name";
        public const string FilterVerdict = "vk.corpus.filter.verdict";
    }

    // ── Metrics ──
    public static class Metrics
    {
        // Gathering
        public const string GatheringCandidateCount  = "vk.corpus.gathering.candidates";
        public const string GatheringDuration        = "vk.corpus.gathering.duration";

        // Filtering
        public const string FilteringPassedCount     = "vk.corpus.filtering.passed";
        public const string FilteringTotalCount      = "vk.corpus.filtering.total";
        public const string FilteringDuration        = "vk.corpus.filtering.duration";
        public const string FilterEvaluationCount    = "vk.corpus.filter.evaluations";

        // Tracking
        public const string TrackingInjectionCount   = "vk.corpus.tracking.injections";
        public const string TrackingDuration         = "vk.corpus.tracking.duration";
    }

    // ── Activities ──
    public static class Activities
    {
        public const string Gathering = "Corpus.Gathering";
        public const string Filtering = "Corpus.Filtering";
        public const string Tracking  = "Corpus.Tracking";
    }
}

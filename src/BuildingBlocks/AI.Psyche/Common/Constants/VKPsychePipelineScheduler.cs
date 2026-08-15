using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Central registry defining the execution topology and scheduling order of all Psyche stages and extensions.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Static constants registry without logic.")]
public static class VKPsychePipelineScheduler
{
    /// <summary>
    /// Stages running BEFORE the LLM call.
    /// </summary>
    public static class Before
    {
        // Extraction Layer (parallel group 1)
        public static readonly VKPipelineSchedule PsycheSessionResolve = new(0, false, null, VKPipelinePhase.Before);
        public static readonly VKPipelineSchedule PsychePreset = new(50, false, null, VKPipelinePhase.Before);
        public static readonly VKPipelineSchedule PsycheProfile = new(100, false, null, VKPipelinePhase.Before);
        public static readonly VKPipelineSchedule PsychePersona = new(100, true, 1, VKPipelinePhase.Before);
        public static readonly VKPipelineSchedule PsycheDirective = new(100, true, 1, VKPipelinePhase.Before);
        public static readonly VKPipelineSchedule PsycheEcho = new(200, false, null, VKPipelinePhase.Before);
        public static readonly VKPipelineSchedule PsycheKnowledge = new(500, true, 2, VKPipelinePhase.Before);

        public static readonly VKPipelineSchedule CorpusGathering = new(540, false, null, VKPipelinePhase.Before);
        public static readonly VKPipelineSchedule CorpusFiltering = new(560, false, null, VKPipelinePhase.Before);
        public static readonly VKPipelineSchedule PsychePattern = new(600, true, 2, VKPipelinePhase.Before);

        // Weaving Layer (sequential)
        public static readonly VKPipelineSchedule PsycheKnowledgeFinalizer = new(990, false, null, VKPipelinePhase.Before);
        public static readonly VKPipelineSchedule PsycheWeaving = new(1000, false, null, VKPipelinePhase.Before);
    }

    /// <summary>
    /// Custom pipeline middlewares order.
    /// </summary>
    public static class Middleware
    {
        public const int EidosContract = 500;
        public const int ContentSafety = 800;
    }

    /// <summary>
    /// Stages running AFTER the LLM call.
    /// </summary>
    public static class After
    {
        public static readonly VKPipelineSchedule PsycheEchoSave = new(900, false, null, VKPipelinePhase.After);
        public static readonly VKPipelineSchedule PsycheSessionUpdate = new(900, false, null, VKPipelinePhase.After);
        public static readonly VKPipelineSchedule UsageRecord = new(int.MaxValue, false, null, VKPipelinePhase.After);
    }
}

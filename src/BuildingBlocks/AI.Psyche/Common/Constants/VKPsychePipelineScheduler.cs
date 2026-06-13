namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Central registry defining the execution topology and scheduling order of all Psyche stages and extensions.
/// </summary>
public static class VKPsychePipelineScheduler
{
    /// <summary>
    /// Stages running BEFORE the LLM call (implements IVKPsycheBeforePipelineStage).
    /// </summary>
    public static class Before
    {
        // Extraction Layer (parallel group 1)
        public static readonly VKStageSchedule Echo = new(0, true, 1);
        public static readonly VKStageSchedule Persona = new(0, true, 1);
        public static readonly VKStageSchedule Directive = new(0, true, 1);

        // Evaluation Layer (Corpus sequential, Knowledge/Pattern parallel group 2)
        public static readonly VKStageSchedule KnowledgeSourcing = new(540, false);
        public static readonly VKStageSchedule CorpusFiltering = new(560, false);
        public static readonly VKStageSchedule Knowledge = new(500, true, 2);
        public static readonly VKStageSchedule Pattern = new(600, true, 2);

        // Weaving Layer (sequential)
        public static readonly VKStageSchedule Weaving = new(1000, false);
    }

    /// <summary>
    /// Custom pipeline middlewares.
    /// </summary>
    public static class Middleware
    {
        public static readonly VKStageSchedule ContentSafety = new(800, false);
    }

    /// <summary>
    /// Stages running AFTER the LLM call (implements IVKPsycheAfterPipelineStage).
    /// </summary>
    public static class After
    {
        public static readonly VKStageSchedule UsageRecord = new(int.MaxValue, false);
    }
}

using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Central registry defining the execution topology and scheduling order of all Vector Search stages and extensions.
/// </summary>
public static class VKVectorSearchPipelineScheduler
{
    /// <summary>
    /// Stages running BEFORE the search terminal action (implements IVKVectorSearchBeforePipelineStage).
    /// </summary>
    public static class Before
    {
        /// <summary>
        /// Schedule configuration for the Query Rewrite stage.
        /// </summary>
        public static readonly VKPipelineStageSchedule QueryRewrite = new(100, false);

        /// <summary>
        /// Schedule configuration for the Semantic Cache stage.
        /// </summary>
        public static readonly VKPipelineStageSchedule SemanticCache = new(200, false);
    }

    /// <summary>
    /// Custom pipeline middlewares order definitions.
    /// </summary>
    public static class Middleware
    {
        /// <summary>
        /// Execution order for the Search Guard middleware.
        /// </summary>
        public const int SearchGuard = 100;
    }

    /// <summary>
    /// Stages running AFTER the search terminal action (implements IVKVectorSearchAfterPipelineStage).
    /// </summary>
    public static class After
    {
        /// <summary>
        /// Schedule configuration for the Rerank stage.
        /// </summary>
        public static readonly VKPipelineStageSchedule Rerank = new(300, false);

        /// <summary>
        /// Schedule configuration for the Context Expansion stage.
        /// </summary>
        public static readonly VKPipelineStageSchedule ContextExpansion = new(400, false);

        /// <summary>
        /// Schedule configuration for the Context Compression stage.
        /// </summary>
        public static readonly VKPipelineStageSchedule ContextCompression = new(500, false);

        /// <summary>
        /// Schedule configuration for the Semantic Cache Write stage.
        /// </summary>
        public static readonly VKPipelineStageSchedule SemanticCacheWrite = new(900, false);
    }
}

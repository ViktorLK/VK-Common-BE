using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Central registry defining the execution topology and scheduling order of all Vector Search stages and extensions.
/// </summary>
public static class VKVectorSearchPipelineScheduler
{
    /// <summary>
    /// Stages running BEFORE the search terminal action.
    /// </summary>
    public static class Before
    {
        /// <summary>
        /// Schedule configuration for the Query Rewrite stage.
        /// </summary>
        public static readonly VKPipelineSchedule QueryRewrite = new(100, false, null, VKPipelinePhase.Before);

        /// <summary>
        /// Schedule configuration for the Semantic Cache stage.
        /// </summary>
        public static readonly VKPipelineSchedule SemanticCache = new(200, false, null, VKPipelinePhase.Before);
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
    /// Stages running AFTER the search terminal action.
    /// </summary>
    public static class After
    {
        /// <summary>
        /// Schedule configuration for the Rerank stage.
        /// </summary>
        public static readonly VKPipelineSchedule Rerank = new(300, false, null, VKPipelinePhase.After);

        /// <summary>
        /// Schedule configuration for the Context Expansion stage.
        /// </summary>
        public static readonly VKPipelineSchedule ContextExpansion = new(400, false, null, VKPipelinePhase.After);

        /// <summary>
        /// Schedule configuration for the Context Compression stage.
        /// </summary>
        public static readonly VKPipelineSchedule ContextCompression = new(500, false, null, VKPipelinePhase.After);

        /// <summary>
        /// Schedule configuration for the Semantic Cache Write stage.
        /// </summary>
        public static readonly VKPipelineSchedule SemanticCacheWrite = new(900, false, null, VKPipelinePhase.After);
    }
}

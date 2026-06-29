using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest;

/// <summary>
/// Central registry defining the execution topology and scheduling order of all Ingest stages.
/// </summary>
public static class VKIngestPipelineScheduler
{
    /// <summary>
    /// Stage order for document loading and parsing.
    /// </summary>
    public static readonly VKPipelineStageSchedule Load = new(100, false);

    /// <summary>
    /// Stage order for generating vector embeddings.
    /// </summary>
    public static readonly VKPipelineStageSchedule Embed = new(200, false);

    /// <summary>
    /// Stage order for writing/indexing documents and vectors to the sink.
    /// </summary>
    public static readonly VKPipelineStageSchedule Write = new(300, false);
}

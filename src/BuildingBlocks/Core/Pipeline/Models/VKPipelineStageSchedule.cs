namespace VK.Blocks.Core;

/// <summary>
/// Declares the scheduling and concurrency configurations for a pipeline stage or middleware.
/// Follows AP.01 (sealed record).
/// </summary>
public sealed record VKPipelineStageSchedule(
    int StageOrder,
    bool IsParallel,
    int? ParallelGroup = null
);

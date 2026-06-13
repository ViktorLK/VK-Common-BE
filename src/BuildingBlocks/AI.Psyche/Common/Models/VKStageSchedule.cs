namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Declares the scheduling and concurrency configurations for a pipeline stage or middleware.
/// Follows AP.01 (sealed record).
/// </summary>
public sealed record VKStageSchedule(
    int Order,
    bool IsParallel,
    int? ParallelGroup = null
);

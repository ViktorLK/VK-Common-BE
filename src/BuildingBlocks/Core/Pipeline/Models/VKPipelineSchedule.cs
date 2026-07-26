namespace VK.Blocks.Core;

/// <summary>
/// Declares explicit scheduling, phase, and concurrency configurations for a pipeline component.
/// Follows AP.01 (sealed record).
/// </summary>
/// <param name="Order">The execution order (lower numbers execute first).</param>
/// <param name="IsParallel">Value indicating whether this component can execute concurrently.</param>
/// <param name="ParallelGroup">Optional parallel group identifier to group concurrent components.</param>
/// <param name="Phase">Execution phase relative to terminal action (None/Before vs After).</param>
public sealed record VKPipelineSchedule(
    int Order,
    bool IsParallel = false,
    int? ParallelGroup = null,
    VKPipelinePhase Phase = VKPipelinePhase.None
);

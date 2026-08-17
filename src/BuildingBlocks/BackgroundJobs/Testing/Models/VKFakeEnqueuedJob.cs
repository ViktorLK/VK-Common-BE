using System;

namespace VK.Blocks.BackgroundJobs;

/// <summary>
/// Record representing an enqueued job captured by the fake scheduler.
/// </summary>
public sealed record VKFakeEnqueuedJob
{
    public required string JobId { get; init; }
    public required Type JobType { get; init; }
    public required object Data { get; init; }
    public VKJobPriority Priority { get; init; }
    public DateTimeOffset EnqueuedAt { get; init; }
}

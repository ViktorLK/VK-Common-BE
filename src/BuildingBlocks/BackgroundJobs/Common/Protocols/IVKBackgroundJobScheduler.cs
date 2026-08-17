using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs;

/// <summary>
/// Unified contract for enqueueing instant, delayed, and continuation jobs.
/// </summary>
public interface IVKBackgroundJobScheduler
{
    Task<VKResult<string>> EnqueueAsync<TJob, TData>(TData data, VKJobPriority priority = VKJobPriority.Default, CancellationToken ct = default)
        where TJob : IVKJob<TData>;

    Task<VKResult<string>> ScheduleAsync<TJob, TData>(TData data, TimeSpan delay, VKJobPriority priority = VKJobPriority.Default, CancellationToken ct = default)
        where TJob : IVKJob<TData>;

    Task<VKResult<string>> ScheduleAsync<TJob, TData>(TData data, DateTimeOffset scheduleAt, VKJobPriority priority = VKJobPriority.Default, CancellationToken ct = default)
        where TJob : IVKJob<TData>;

    Task<VKResult<string>> ContinueWithAsync<TJob, TData>(string parentJobId, TData data, CancellationToken ct = default)
        where TJob : IVKJob<TData>;
}

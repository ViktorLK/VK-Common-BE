using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs.Testing.Internal;

internal sealed class FakeBackgroundJobScheduler : IVKFakeBackgroundJobScheduler
{
    private readonly ConcurrentBag<VKFakeEnqueuedJob> _jobs = new();
    private readonly IVKGuidGenerator _guidGenerator;

    public FakeBackgroundJobScheduler(IVKGuidGenerator guidGenerator)
    {
        _guidGenerator = VKGuard.NotNull(guidGenerator);
    }

    public IReadOnlyList<VKFakeEnqueuedJob> EnqueuedJobs => _jobs.ToList();

    public void Reset() => _jobs.Clear();

    public Task<VKResult<string>> EnqueueAsync<TJob, TData>(TData data, VKJobPriority priority = VKJobPriority.Default, CancellationToken ct = default)
        where TJob : IVKJob<TData>
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        var jobId = _guidGenerator.Create().ToString("N");
        _jobs.Add(new VKFakeEnqueuedJob
        {
            JobId = jobId,
            JobType = typeof(TJob),
            Data = data,
            Priority = priority,
            EnqueuedAt = DateTimeOffset.UtcNow
        });
        return Task.FromResult(VKResult.Success(jobId));
    }

    public Task<VKResult<string>> ScheduleAsync<TJob, TData>(TData data, TimeSpan delay, VKJobPriority priority = VKJobPriority.Default, CancellationToken ct = default)
        where TJob : IVKJob<TData>
    {
        return EnqueueAsync<TJob, TData>(data, priority, ct);
    }

    public Task<VKResult<string>> ScheduleAsync<TJob, TData>(TData data, DateTimeOffset scheduleAt, VKJobPriority priority = VKJobPriority.Default, CancellationToken ct = default)
        where TJob : IVKJob<TData>
    {
        return EnqueueAsync<TJob, TData>(data, priority, ct);
    }

    public Task<VKResult<string>> ContinueWithAsync<TJob, TData>(string parentJobId, TData data, CancellationToken ct = default)
        where TJob : IVKJob<TData>
    {
        VKGuard.NotNullOrWhiteSpace(parentJobId);
        return EnqueueAsync<TJob, TData>(data, VKJobPriority.Default, ct);
    }
}

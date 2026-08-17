using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.BackgroundJobs.Shared;
using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs.Jobs.Internal;

internal sealed class DefaultBackgroundJobScheduler : IVKBackgroundJobScheduler
{
    private readonly IVKJobStore _jobStore;
    private readonly IVKJobStateStore _stateStore;
    private readonly JobPayloadSerializer _serializer;
    private readonly IVKGuidGenerator _guidGenerator;

    public DefaultBackgroundJobScheduler(
        IVKJobStore jobStore,
        IVKJobStateStore stateStore,
        JobPayloadSerializer serializer,
        IVKGuidGenerator guidGenerator)
    {
        _jobStore = VKGuard.NotNull(jobStore);
        _stateStore = VKGuard.NotNull(stateStore);
        _serializer = VKGuard.NotNull(serializer);
        _guidGenerator = VKGuard.NotNull(guidGenerator);
    }

    public async Task<VKResult<string>> EnqueueAsync<TJob, TData>(TData data, VKJobPriority priority = VKJobPriority.Default, CancellationToken ct = default)
        where TJob : IVKJob<TData>
    {
        var jobId = _guidGenerator.Create().ToString("N");
        var payload = new VKJobPayload
        {
            JobType = typeof(TJob).AssemblyQualifiedName ?? typeof(TJob).FullName ?? typeof(TJob).Name,
            SerializedData = _serializer.Serialize(data),
            CreatedAt = DateTimeOffset.UtcNow
        };

        var queueName = priority.ToString().ToLowerInvariant();
        var saveResult = await _jobStore.SaveJobAsync(jobId, payload, queueName, ct).ConfigureAwait(false);
        if (!saveResult.IsSuccess)
        {
            return VKResult.Failure<string>(saveResult.Errors);
        }

        await _stateStore.SetStateAsync(jobId, VKJobState.Enqueued, null, ct).ConfigureAwait(false);
        return VKResult.Success(jobId);
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

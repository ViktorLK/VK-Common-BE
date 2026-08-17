using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs.Recurring.Internal;

internal sealed class DefaultRecurringJobScheduler : IVKRecurringJobScheduler
{
    private readonly ConcurrentDictionary<string, (VKCronExpression Cron, string Queue)> _recurringJobs = new();

    public Task<VKResult> AddOrUpdateAsync<TJob, TData>(string recurringJobId, TData data, VKCronExpression cronExpression, string queue = "default", CancellationToken ct = default)
        where TJob : IVKJob<TData>
    {
        VKGuard.NotNullOrWhiteSpace(recurringJobId);
        VKGuard.NotNull(cronExpression);

        _recurringJobs[recurringJobId] = (cronExpression, queue);
        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult> RemoveAsync(string recurringJobId, CancellationToken ct = default)
    {
        VKGuard.NotNullOrWhiteSpace(recurringJobId);
        _recurringJobs.TryRemove(recurringJobId, out _);
        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult> TriggerAsync(string recurringJobId, CancellationToken ct = default)
    {
        VKGuard.NotNullOrWhiteSpace(recurringJobId);
        if (!_recurringJobs.ContainsKey(recurringJobId))
        {
            return Task.FromResult(VKResult.Failure(VKJobErrors.NotFound));
        }

        return Task.FromResult(VKResult.Success());
    }
}

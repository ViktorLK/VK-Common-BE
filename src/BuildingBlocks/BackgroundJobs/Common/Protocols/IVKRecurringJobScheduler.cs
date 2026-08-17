using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs;

/// <summary>
/// Unified contract for recurring/Cron job scheduling.
/// </summary>
public interface IVKRecurringJobScheduler
{
    Task<VKResult> AddOrUpdateAsync<TJob, TData>(string recurringJobId, TData data, VKCronExpression cronExpression, string queue = "default", CancellationToken ct = default)
        where TJob : IVKJob<TData>;

    Task<VKResult> RemoveAsync(string recurringJobId, CancellationToken ct = default);

    Task<VKResult> TriggerAsync(string recurringJobId, CancellationToken ct = default);
}

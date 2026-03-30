using System.Diagnostics;
using Hangfire;
using VK.Blocks.BackgroundJobs.Abstractions;
using VK.Blocks.BackgroundJobs.Abstractions.Contracts;
using VK.Blocks.MultiTenancy.Abstractions;
using VK.Blocks.MultiTenancy.Abstractions.Contracts;
using VK.Blocks.MultiTenancy.Context;

namespace VK.Blocks.BackgroundJobs.Hangfire.Services;

public sealed class HangfireRecurringJobService(
    IRecurringJobManager recurringJobManager,
    ITenantProvider tenantProvider) : IRecurringJobService
{
    public void AddOrUpdate<TJob, TData>(
        string jobId, 
        TData data, 
        string cronExpression, 
        JobRecurringOptions? options = null) where TJob : IJobHandler<TData>
    {
        var tenantId = tenantProvider.GetCurrentTenantId();
        var correlationId = Activity.Current?.Id;
        
        var hangfireOptions = new global::Hangfire.RecurringJobOptions
        {
            TimeZone = options?.TimeZoneId != null 
                ? TimeZoneInfo.FindSystemTimeZoneById(options.TimeZoneId) 
                : TimeZoneInfo.Utc,
            QueueName = options?.QueueName ?? "default"
        };

        recurringJobManager.AddOrUpdate<HangfireJobRunner>(
            jobId,
            runner => runner.RunAsync<TJob, TData>(data, tenantId, correlationId, CancellationToken.None),
            cronExpression,
            hangfireOptions);
    }

    public void Remove(string jobId)
    {
        recurringJobManager.RemoveIfExists(jobId);
    }
}

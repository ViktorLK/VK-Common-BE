using VK.Blocks.BackgroundJobs.Abstractions.Contracts;

namespace VK.Blocks.BackgroundJobs.Abstractions;

/// <summary>
/// Service to manage recurring (scheduled) jobs.
/// </summary>
public interface IRecurringJobService
{
    /// <summary>
    /// Adds or updates a recurring job.
    /// </summary>
    /// <typeparam name="TJob">The type of the job handler.</typeparam>
    /// <typeparam name="TData">The type of the job data.</typeparam>
    /// <param name="jobId">A unique identifier for the recurring job.</param>
    /// <param name="data">The data for the job.</param>
    /// <param name="cronExpression">The CRON expression for scheduling.</param>
    /// <param name="options">Optional configuration for the recurring job.</param>
    void AddOrUpdate<TJob, TData>(
        string jobId, 
        TData data, 
        string cronExpression, 
        JobRecurringOptions? options = null) where TJob : IJobHandler<TData>;

    /// <summary>
    /// Removes a recurring job.
    /// </summary>
    /// <param name="jobId">The unique identifier of the recurring job.</param>
    void Remove(string jobId);
}

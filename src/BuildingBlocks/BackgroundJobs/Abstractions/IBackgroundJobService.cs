using VK.Blocks.Core.Results;

namespace VK.Blocks.BackgroundJobs.Abstractions;

/// <summary>
/// Service to enqueue background jobs.
/// </summary>
public interface IBackgroundJobService
{
    /// <summary>
    /// Enqueues a job to be executed as soon as possible.
    /// </summary>
    /// <typeparam name="TJob">The type of the job handler.</typeparam>
    /// <typeparam name="TData">The type of the job data.</typeparam>
    /// <param name="data">The data for the job.</param>
    /// <returns>The unique identifier of the enqueued job.</returns>
    string Enqueue<TJob, TData>(TData data) where TJob : IJobHandler<TData>;
}

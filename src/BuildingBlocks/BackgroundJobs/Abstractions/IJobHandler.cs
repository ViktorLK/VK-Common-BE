using VK.Blocks.BackgroundJobs.Abstractions.Contracts;

namespace VK.Blocks.BackgroundJobs.Abstractions;

/// <summary>
/// Interface for a background job handler.
/// </summary>
/// <typeparam name="TData">The type of the job data.</typeparam>
public interface IJobHandler<in TData>
{
    /// <summary>
    /// Executes the job.
    /// </summary>
    /// <param name="data">The job data.</param>
    /// <param name="context">The job execution context.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The result of the job execution.</returns>
    Task<JobExecutionResult> ExecuteAsync(TData data, JobContext context, CancellationToken ct);
}

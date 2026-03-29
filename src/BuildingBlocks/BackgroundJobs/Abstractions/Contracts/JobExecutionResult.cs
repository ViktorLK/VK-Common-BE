namespace VK.Blocks.BackgroundJobs.Abstractions.Contracts;

/// <summary>
/// Represents the result of a job execution.
/// </summary>
public enum JobExecutionResult
{
    /// <summary>The job completed successfully.</summary>
    Success = 0,

    /// <summary>The job failed and should not be retried.</summary>
    Failed = 1,

    /// <summary>The job failed and should be retried.</summary>
    Retry = 2
}

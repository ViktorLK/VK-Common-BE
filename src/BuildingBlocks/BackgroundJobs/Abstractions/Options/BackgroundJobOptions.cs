namespace VK.Blocks.BackgroundJobs.Abstractions.Options;

/// <summary>
/// Configuration options for background job processing.
/// </summary>
public sealed class BackgroundJobOptions
{
    public const string SectionName = "BackgroundJobs";

    /// <summary>
    /// Gets or sets the maximum number of retries for a failed job.
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// Gets or sets the default queue name.
    /// </summary>
    public string DefaultQueue { get; set; } = "default";

    /// <summary>
    /// Gets or sets the default timeout for a job execution.
    /// </summary>
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromMinutes(30);
}

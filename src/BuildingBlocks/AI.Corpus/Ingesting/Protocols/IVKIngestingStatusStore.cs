namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Store for tracking ingestion job statuses.
/// </summary>
public interface IVKIngestingStatusStore
{
    /// <summary>
    /// Updates the status of a specific job.
    /// </summary>
    void UpdateStatus(string jobId, VKIngestingStatus status, string? errorMessage = null);

    /// <summary>
    /// Gets the status of a specific job.
    /// </summary>
    VKIngestingJobStatus? GetStatus(string jobId);
}

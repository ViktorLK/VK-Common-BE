namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Status of an ingestion job.
/// </summary>
public enum VKIngestingStatus
{
    /// <summary>
    /// The job is pending.
    /// </summary>
    Pending,

    /// <summary>
    /// The job is currently processing.
    /// </summary>
    Processing,

    /// <summary>
    /// The job has completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// The job has failed.
    /// </summary>
    Failed
}
